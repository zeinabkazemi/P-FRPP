using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace WorstCaseReleaseScenario
{
    public class CPermutations
    {

        private ArrayList allPermutations;
        int[] newList = null;
        /// <summary>
        /// Get all permutations of number in range [min,max]
        /// eg. if min =2 and max = 4
        /// Program should return {2,3,4}, {2,4,3}, {3,2,4}, {3,4,2}, {4,3,2}, {4,2,3}
        /// /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public ArrayList GetPermutations(int min, int max)
        {
            int ii;
             
            //Store in an array
            int[] list = new int[max - min + 1];
            for (ii = 0; ii <= max - min; ii++)
                list[ii] = min + ii;

            //Set up global variable
            allPermutations = new ArrayList();
            newList = new int[list.Length];

            GeneratePermutations(list,0);
            return allPermutations;
        }


        /// <summary>
        /// Assume list = {1,2,3}
        /// </summary>
        /// <param name="list"></param>
        /// <param name="idx"></param>
        private void GeneratePermutations(int [] list, int idx)
        {
            int ii,jj;
            int[] tmpList;
            int tmpIdx;

            //If there are only 2 elements in List
            if (list.Length == 2)
            {
                newList[idx] = list[1];
                newList[idx + 1] = list[0];

                StoreInArrayList(newList);

                newList[idx] = list[0];
                newList[idx + 1] = list[1];

                StoreInArrayList(newList);
            }
            else
            {
                for (ii = 0; ii < list.Length; ii++)
                {
                    newList[idx] = list[ii]; //Change the first element
                    tmpList = new int[list.Length - 1];
                    tmpIdx = 0;
                    for (jj = 0; jj < list.Length ; jj++)
                    {
                        if (jj != ii)
                            tmpList[tmpIdx++] = list[jj];
                    }
                    GeneratePermutations(tmpList, idx + 1);

                }

            }
        }

        private void StoreInArrayList(int[] elements)
        {
            ArrayList permutation = new ArrayList();
            foreach (int i in elements)
                permutation.Add(i);

            allPermutations.Add(permutation);
        }
    }
}
