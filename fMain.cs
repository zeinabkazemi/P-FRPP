using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PFRPLib;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WorstCaseReleaseScenario
{
    public partial class fMain : Form
    {
        private CReleaseOrderings getReleaseOrder;
        private int  worstResponseTime = 0;
        private CEvent lowestPriority;
        private int[] worstOrdering = null;
        private double totalOrderings;
        private int count;
        private int noOfUnschedulableOrderings;
        private SortedList allEvents;
        private int maxReleaseTime = 0, maxTimeToAnalyze=0;
        double startTick;
        double elapsedTimeStartTick;
        int timeMeasureMentIdx;
        ArrayList interferences;

        public static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);
                return task.IsCompleted;
            }
            catch
            {
                return false;
            }
        }

        public fMain()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            textFileName.Text = EIServiceLibrary.CFileOperations.showFileOpen("Event Set File", 
                "txt", "taskset.txt");
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            textFileName.Text = "";
            textGroupFolder.Text= "";
            
        }

        private void CleanUp()
        {
            textInterferences.Text = "";
            textOrdering.Text = "";
            textUpperBoundResponseTime_Improved.Text = "";
            textWorstResponseTime.Text = "";
            textUtilization.Text = "";
            textInterferences.Text = "";
            textNoOfUnschedulableEvents.Text = "";

            worstResponseTime = 0;
            lowestPriority=null;
            totalOrderings=0;
            count=0;
            noOfUnschedulableOrderings=0;
            maxReleaseTime = 0;
            maxTimeToAnalyze = 0;
            startTick=0;
            elapsedTimeStartTick=0;
            timeMeasureMentIdx=0;

            getReleaseOrder = null;
            allEvents = null;
        
        }

        

        private void Start()
        {
            bool status;
           
            
            PFRPLib.CSchedAnalysis sa = new CSchedAnalysis();
            
            
            CleanUp();
            CEventSetTools es = new CEventSetTools();
       
            getReleaseOrder = new CReleaseOrderings();
            

            interferences = new ArrayList();

            //Read events Set
            allEvents = es.readEventSet(textFileName.Text);
            this.Text = "Worst Case Response Time - " + allEvents.Count.ToString() + " event(s)";
            
            //Compute Utilization
            textUtilization.Text = sa.GetUtilization(allEvents).ToString();
            
            //Get Response Upper Bound
            CUpperBoundIterativeMethod upperBound = new CUpperBoundIterativeMethod(); //CUpperBoundIterativeMethod();
            textUpperBoundResponseTime.Text = upperBound.getEventCompletionTime(allEvents).ToString();


            CUpperBoundRTCSA rtcsa = new CUpperBoundRTCSA();
            textUpperBoundJim.Text = rtcsa.getEventCompletionTime(allEvents).ToString();

            CUpperBoundIterative_2 upperBound_Improved = new CUpperBoundIterative_2();
            textUpperBoundResponseTime_Improved.Text = upperBound_Improved.getEventCompletionTime(allEvents).ToString();

            CUpperBound_072010 upperBound_072010 = new CUpperBound_072010();
            textUpperBound072010.Text = upperBound_072010.getEventCompletionTime(allEvents).ToString();

            CLowerBound lowerBound = new CLowerBound();
            textLowerBound.Text = lowerBound.getEventCompletionTime(allEvents).ToString();
            textLowerBound.Refresh();

            //----------------------------- Offset based WCRT
            if (checkOffsetBasedWCRT.Checked)
            {
                CReleaseOffsetWCRT offsetWCRT = new CReleaseOffsetWCRT();
                //CReleaseOffsetWCRT.PassMessage += new CReleaseOffsetWCRT.passMessage(UpdateStatus);
                offsetWCRT.maxTimeToAnalyze = Convert.ToInt64(textUpperBound072010.Text);

                bool Completed = ExecuteWithTimeLimit(TimeSpan.FromMinutes(4), () =>
                {
                    textOffsetBasedWCRT.Text = offsetWCRT.getEventCompletionTime(allEvents).ToString();
                });
                if (!Completed)
                {
                    textOffsetBasedWCRT.Text = "-2";
                }

                textOffsetWCRT_NumberOrderings.Text = offsetWCRT.maxOrderings.ToString();
                textOffsetUpperBound.Text = offsetWCRT.offsetUpperBound.ToString();
                textOffsetLowerBound.Text = offsetWCRT.offsetLowerBound.ToString();
                textWCRTOrdering.Text = getOrderingText(offsetWCRT.worstOrdering);
            }
            else
                textOffsetBasedWCRT.Text = "-1";

            //Zeinab:changed
            Application.DoEvents();
            return;
            
            //-----------------------------------------------------------------------

            //Set Max Time for release orderings
            if(textMaxResponseTime.Text.Trim() == "0")
                maxReleaseTime = Convert.ToInt32(textUpperBoundResponseTime_Improved.Text);
            else
                maxReleaseTime = Convert.ToInt32(textMaxResponseTime.Text);


            //Always set max time to analyze as the upper bound
            maxTimeToAnalyze = Convert.ToInt32(textUpperBoundResponseTime_Improved.Text);

            textUpperBoundResponseTime_Improved.ForeColor = Color.Red;
            //--------------------------------------------------------------------

            lowestPriority = (CEvent)allEvents.GetByIndex(0);
            lowestPriority.R_releaseTime = 0;

            //Get Release Ordering
            UpdateStatus("Start Generating list of priority orderings");
            totalOrderings = Math.Pow((double)(maxReleaseTime + 1),
                    (double)allEvents.Count - 1);
            textTotalOrderingsWCRT.Text = totalOrderings.ToString();
            //---------------------------------------------

            count = 0;

            noOfUnschedulableOrderings = 0;

            timeMeasureMentIdx = 1;
            startTick = DateTime.Now.Ticks;
            elapsedTimeStartTick = DateTime.Now.Ticks;

            //Update Max Time To Run
            lblMaxTime.Text = "Max Release Time: " + maxReleaseTime.ToString();

            maxReleaseTime = Convert.ToInt32( textOffsetBasedWCRT.Text);

            if (maxReleaseTime > 0)
            {
                maxReleaseTime = maxReleaseTime * 2;
                CReleaseOrderings.allOrderings += new CReleaseOrderings.orderingCreated(runOrdering);
                status = getReleaseOrder.GetReleaseOrderings(allEvents.Count - 1, 0, maxReleaseTime);
                CReleaseOrderings.allOrderings -= new CReleaseOrderings.orderingCreated(runOrdering);
            }
            else
            {
                textWorstResponseTime.Text = "-1";
            }
            //Will display ordering automatically

            //Add Interferences for last
            AddInterferences(interferences);
        }


        private void runOrdering(int [] ordering)
        {
                CEvent E;
                CTimeAccurateSimulation timeAccurate=null ;
                int jj;
                int responseTime=0;
                int maxTime=0;
                double endTick, timeInSec, timeInHours, orderingsPerTimeMeasure;
                SortedList tmpEvents = new SortedList();
                
                Application.DoEvents();
                orderingsPerTimeMeasure = 10000;


                //Measure every orderingsPerTimeMeasure
                if (timeMeasureMentIdx++ == orderingsPerTimeMeasure)
                {
                    endTick = DateTime.Now.Ticks;
                    //time in sec for each ordering
                    timeInSec = ((endTick - startTick) / orderingsPerTimeMeasure)/
                        (long)Math.Pow(10.0, 7.0);
                    //1 tick = 100 nanoseconds interval
                    
                    timeInHours = (timeInSec / 3600); //Time in hours for each ordering
                    lblEstimatedtime.Text = "Estimated minimum time(hrs): " + (timeInHours * totalOrderings).ToString();
                    timeMeasureMentIdx = 1;
                    startTick = DateTime.Now.Ticks;

                    //Compute elapsedTime
                    timeInSec = (endTick - elapsedTimeStartTick) / (long)Math.Pow(10.0, 7.0);
                    timeInHours = (timeInSec / 3600);

                    lblElaspedTime.Text = "Elapsed Time (hrs): " + timeInHours.ToString();
                    System.GC.Collect(); //Run GC
                    AddInterferences(interferences);
                }

               
                UpdateStatus("Performed " + (++count).ToString() + " of minimum " + totalOrderings);
                
                //Add lowest priority event first                
                tmpEvents.Add(lowestPriority.i_Priority, lowestPriority);

                for(jj=1;jj<allEvents.Count;jj++)
                {
                    //0 is lowest priority task
                    E = (CEvent) allEvents.GetByIndex(jj);
                    E.R_releaseTime = ordering[jj-1]; //Ordering[0] is for next lowest priority event and so on
                    tmpEvents.Add(E.i_Priority, E);
                }
                
                //Run the simulation
                //Create a new object at every instance

                //Run the simulation fo extended time till the event completes
                jj = 1;
                responseTime = -1; maxTime=maxTimeToAnalyze;
                while (responseTime < 0 && maxTime <= 3 * maxTimeToAnalyze)
                {
                    timeAccurate = new CTimeAccurateSimulation();
                    responseTime = (int)timeAccurate.findResponseTime(tmpEvents, 1, (long)maxTime);
                    jj = jj + 3;
                    maxTime = maxReleaseTime * jj;
                }

                if (responseTime < 0)
                {
                    noOfUnschedulableOrderings++;
                    textNoOfUnschedulableEvents.Text = noOfUnschedulableOrderings.ToString();
                    showUnschedulableOrdering(ordering, responseTime * -1, (int)timeAccurate.currentTime);
                    Application.DoEvents();
                }
                    

                if (responseTime > worstResponseTime)
                {
                    worstOrdering = ordering;
                    worstResponseTime = responseTime;
                    textWorstResponseTime.Text = worstResponseTime.ToString();
                    textInterferencesForWCRT.Text = timeAccurate.noOfInterferencesLowestPriority.ToString();
                    
                    Application.DoEvents();
                    showWorstOrdering(worstOrdering, worstResponseTime, timeAccurate.noOfInterferencesLowestPriority);
                }

                //Interference Delays the computation hence add only if users selects so
                if (checkInterference.Checked)
                {
                    CInterference oInt = new CInterference();
                    oInt.noOfTotalInterferences = timeAccurate.totalInterferences;
                    oInt.RT = responseTime;
                    interferences.Add(oInt);
                }
                
        }


        private void AddInterferences(ArrayList list)
        {
            if (checkInterference.Checked)
            {
                string sMain = "";
                foreach (CInterference oInt in list)
                {
                    sMain = sMain + oInt.noOfTotalInterferences.ToString() +
                         "\t" + oInt.RT.ToString() + "\r\n";
                }

                textInterferences.Text = "Interference" + "\t" + "WCRT" + "\r\n" + sMain;
            }

        }

        private void showUnschedulableOrdering(int[] ordering, int unSchedulableEvent, int currentTime)
        {
            int ii;
            string main = "------------------>" + "Event :" + unSchedulableEvent.ToString() 
                                + " Time: " + currentTime.ToString() + "\r\n";

            if (ordering == null)
            {
                textUnschedulableEvents.Text = "No release time possible";
                return;
            }

            for (ii = 0; ii < ordering.Length; ii++)
            {
                main = main + "Priority: " + (ii + 2).ToString() + ", Release " + ordering[ii] + "\r\n";
            }

            textUnschedulableEvents.Text = textUnschedulableEvents.Text + main;

            textWorstResponseTime.Text = worstResponseTime.ToString();
        }
        


        private void showWorstOrdering(int[] ordering, int worstResponseTime, int inteferenceCount)
        {
            
            string main = "------------>" + "Time :" + worstResponseTime.ToString() + "\r\n";

            main = main + "------------>" + "Low Priority Interference Count:" + inteferenceCount.ToString() + "\r\n";
            if (ordering == null)
            {
                textOrdering.Text = "No release time possible";
                return;
            }



            textOrdering.Text = textOrdering.Text + main + getOrderingText(ordering);

            textWorstResponseTime.Text = worstResponseTime.ToString();
        }

        private string getOrderingText(int[] ordering)
        {
            string main="";
            if (ordering != null)
            {
                for (int ii = 0; ii < ordering.Length; ii++)
                {
                    main = main + "Priority: " + (ii + 2).ToString() + ", Release " + ordering[ii] + "\r\n";
                }
            }
            return main;
        }

        private void UpdateStatus(string status)
        {
            lblStatus.Text = status;
            lblStatus.Refresh();
        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            buttonStop_Click(sender, e);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textWorstResponseTime_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textUpperBoundResponseTime_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //Temp Test of CPermutations
            //ArrayList tmpA;
            //CPermutations per = new CPermutations();
            //tmpA = per.GetPermutations(2, 7);

            
            Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            try
            {
                getReleaseOrder.shutDown = true;
                getReleaseOrder = null;
            }
            catch { }
        }

        private void btnBrowseGroup_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath= "C:\\Documents and Settings\\Chaitanya Belwal\\My Documents\\Research\\Papers\\BoundsOnResponseTime_PFRP\\Documents\\TestCases";
            folderBrowserDialog1.ShowDialog();
            textGroupFolder.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnStartGroup_Click(object sender, EventArgs e)
        {
            int iRTCSA=0;
            DirectoryInfo di = new DirectoryInfo(textGroupFolder.Text);
            FileInfo[] rgFiles = di.GetFiles("*.txt");


            textGroupResults.Text = "FileName\tUtilization\tUBRT\tNewUBRT\tOffsetWCRT\tWCRT\t#OffsetWCRT\t#WCRT\tLB\tUB\r\n";

            foreach (FileInfo fi in rgFiles)
            {
                //textMaxResponseTime.Text = "1";
                textFileName.Text = fi.FullName;
                Start();


                //Thread.Sleep(1000);
                //At this point computation is done
                textGroupResults.Text = textGroupResults.Text + fi.Name + "\t" + textUtilization.Text + "\t" +
                                        textUpperBoundResponseTime_Improved.Text + "\t" + textUpperBound072010.Text + "\t" +
                                        textOffsetBasedWCRT.Text + "\t" +
                                        textWorstResponseTime.Text + "\t" +
                                        textOffsetWCRT_NumberOrderings.Text + "\t" + textTotalOrderingsWCRT.Text + 
                                        "\t" + textOffsetLowerBound.Text + "\t" + textOffsetUpperBound.Text +  "\r\n";

                if(textNoOfUnschedulableEvents.Text.Trim() == "")
                    if (textUpperBoundJim.Text.Trim() != "-1") iRTCSA++;


                //if (Convert.ToInt32(textUpperBound072010.Text) < Convert.ToInt32(textOffsetBasedWCRT.Text))
                //    MessageBox.Show("New UB-WCRT < WCRT");

            }

            UpdateStatus("Total solved by RTCSA : " + iRTCSA.ToString());
            //Iterate through every folder in the Group Folder
            
            
        }

        private void textOffsetBound_TextChanged(object sender, EventArgs e)
        {

        }

        




    }
}
