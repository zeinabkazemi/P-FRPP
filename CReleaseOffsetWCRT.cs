using System;
using System.Collections;
using System.Linq;
using System.Text;
using PFRPLib;
using System.Windows.Forms;

namespace WorstCaseReleaseScenario
{
    public class CReleaseOffsetWCRT
    {
        
        private long WCRT;
        private SortedList allEvents;
        private CEvent lowestPriority;
        private long totalOrderings;
        private CReleaseOrderings getReleaseOrder;

        public long maxOrderings;
        public int[] worstOrdering;
        public long offsetUpperBound;
        public long offsetLowerBound;
        public long maxTimeToAnalyze;


        public delegate void passMessage(string Message);
        public static event passMessage PassMessage;

        public CReleaseOffsetWCRT()
        {
            maxTimeToAnalyze = 0;
        }

        public int getEventCompletionTime(SortedList eventSet)
        {
            
            
            CSchedAnalysis sa = new CSchedAnalysis();
            bool status;
            
            CReleaseOrderings.allOrderings += new CReleaseOrderings.orderingCreated(runOrderingWCRT);
            CReleaseOffsetBound offsetBound = new CReleaseOffsetBound();
            offsetUpperBound = offsetBound.getReleaseOffsetUpperBound(eventSet);
            offsetLowerBound = offsetBound.getReleaseOffsetLowerBound(eventSet);

            totalOrderings = (long)Math.Pow((double)((offsetUpperBound - offsetLowerBound)),
                    (double)eventSet.Count - 1);

            allEvents = eventSet;

            if(maxTimeToAnalyze==0) maxTimeToAnalyze = sa.GetHyperPeriod(eventSet)/2;

            getReleaseOrder = new CReleaseOrderings();

            lowestPriority = (CEvent)allEvents.GetByIndex(0);
            lowestPriority.R_releaseTime = 0;
            WCRT = 0;
            maxOrderings = 0;
            status = getReleaseOrder.GetReleaseOrderings(eventSet.Count - 1, (int)offsetLowerBound, (int)offsetUpperBound);
            CReleaseOrderings.allOrderings -= new CReleaseOrderings.orderingCreated(runOrderingWCRT);
            return (int)WCRT;
        }


        private void runOrderingWCRT(int[] ordering)
        {
            CEvent E;
            CTimeAccurateSimulation timeAccurate = null;
            int jj;
            int responseTime = 0;

            maxOrderings++;

            //PassMessage("Evaluated " + maxOrderings.ToString() + " of " + totalOrderings.ToString());

            if (WCRT >= 0)
            {
                SortedList tmpEvents = new SortedList();

                tmpEvents.Add(lowestPriority.i_Priority, lowestPriority);

                for (jj = 1; jj < allEvents.Count; jj++)
                {
                    //0 is lowest priority task
                    E = (CEvent)allEvents.GetByIndex(jj);
                    E.R_releaseTime = ordering[jj - 1]; //Ordering[0] is for next lowest priority event and so on
                    tmpEvents.Add(E.i_Priority, E);
                }
                timeAccurate = new CTimeAccurateSimulation();
               responseTime = (int)timeAccurate.findResponseTime(tmpEvents, 1, maxTimeToAnalyze*3);
                
                //Get highest non-Zero WCRT
                if (responseTime < 0)
                {
                   WCRT = -1;
                   getReleaseOrder.shutDown = true;
                }
                else if (responseTime > WCRT)
                {
                    worstOrdering = ordering;
                    WCRT = responseTime;
                    Application.DoEvents();
                }
            }

        }
    }
}
