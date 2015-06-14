using System;
using System.Collections;
using System.Linq;
using System.Text;
using EIServiceLibrary;
using System.IO;
using PFRPLib;

namespace WorstCaseReleaseScenario
{
    public class CLowerBound
    {
        public int getEventCompletionTime(SortedList eventSet)
        {
            int LBRT, LBRT_1;
            int sum = 0, n_hp;

            CEvent hp_Event;
            CEvent eventLowestPriority;

            int ii;

            n_hp = eventSet.Count;
            eventLowestPriority = (CEvent)eventSet[1];

            LBRT = 0;// eventLowestPriority.P_processingTime;
            LBRT_1 = 1;

            //Compute Max Interference Costs to get Initial Value
            while (LBRT != LBRT_1) //When both match solution has converged
            {

                sum = 0;
                for (ii = n_hp; ii > 1; ii--)
                {
                    hp_Event = (CEvent)eventSet[ii];

                    sum = sum + (int)Math.Ceiling((double)(LBRT) /
                            (double)hp_Event.W_minimumWaitTime) * hp_Event.P_processingTime; //RM

                }

                LBRT_1 = LBRT;
                LBRT = eventLowestPriority.P_processingTime + sum;

                if (LBRT > 1000) return -1;
                //if  convergence limit reached abort and return -1
            }
            return LBRT;
        }
              
    }
    
}
