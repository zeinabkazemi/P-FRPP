using System;
using System.Collections;
using System.Linq;
using System.Text;
using PFRPLib;

namespace WorstCaseReleaseScenario
{
    public class CUpperBound_072010
    {
        /// <summary>
        /// Will compute response time of event with the lowest priority
        /// </summary>
        /// <param name="eventSet"></param>
        /// <returns></returns>
        public int getEventCompletionTime(SortedList eventSet)
        {
            long UB_WCRT,  lastUB_WCRT=0, lCumulativeCost = 0;
            int n_hp, n_jobs;
            CReleaseOffsetBound offsetBound = new CReleaseOffsetBound();

            CEvent  current_Event=null, hp_Event = null;
            CEvent eventLowestPriority;

            int ii, jj;
            n_hp = eventSet.Count;
            eventLowestPriority = (CEvent)eventSet[1];
            UB_WCRT = offsetBound.getReleaseOffsetUpperBound(eventSet);

            //Compute Cumulative Cost from each higher priority task
            //for (ii = n_hp - 1; ii >= 1; ii--)
            for (ii = 2; ii <= n_hp+1; ii++)
            {
                

                lCumulativeCost = 0;
                //Recompute UB_WCRT based on cumulative costs from all previous caes
                for (jj = 2; jj <= ii-1; jj++)
                {
                    hp_Event = (CEvent)eventSet[jj];
                    n_jobs = (int)Math.Ceiling((double)(UB_WCRT - lastUB_WCRT) / (double)hp_Event.W_minimumWaitTime);
                    lCumulativeCost = lCumulativeCost + GetCost(eventSet, jj) * n_jobs;
                }

                
                UB_WCRT = lCumulativeCost + UB_WCRT;
                //Compute Cumulative Cost From Processing Time
                //lCumulativeCost = lCumulativeCost + GetCost(eventSet,ii);

                if (ii <= n_hp)
                {
                    current_Event = (CEvent)eventSet[ii];
                    n_jobs = (int)Math.Ceiling((double)UB_WCRT / (double)current_Event.W_minimumWaitTime);
                    lastUB_WCRT = UB_WCRT;
                    UB_WCRT = UB_WCRT + GetCost(eventSet, ii) * n_jobs;
                }
            }

            
            
            return (int)UB_WCRT; // + eventLowestPriority.P_processingTime;


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventSet"></param>
        /// <param name="iTaskIdx"></param>
        /// <returns></returns>
        private long GetCost(SortedList eventSet, int iTaskIdx)
        {
            CEvent hp_Event = null, lp_Event = null;
            

            hp_Event = (CEvent)eventSet[iTaskIdx];
            lp_Event = (CEvent)eventSet[1];
            return hp_Event.P_processingTime + lp_Event.P_processingTime - 1;

            //for (jj = 2; jj <= iTaskIdx; jj++)
            //{
            //    hp_Event = (CEvent)eventSet[jj];

            //    lCumulativeCost = lCumulativeCost +
            //                hp_Event.P_processingTime;
            //}

            ////Compute Cumulative Cost From Max Abort
            //for (jj = 1; jj <= iTaskIdx - 1; jj++)
            //{
            //    hp_Event = (CEvent)eventSet[jj];

            //    lCumulativeCost = lCumulativeCost +
            //                 (hp_Event.P_processingTime - 1);
            //}

            //return lCumulativeCost;
        }
    }
}
