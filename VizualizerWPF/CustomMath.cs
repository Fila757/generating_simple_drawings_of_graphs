using System;
using System.Collections.Generic;
using System.Text;

namespace VizualizerWPF
{
    static class CustomMath
    {
        public static int CombCoeff(int n, int k)
        {
            int res = 1;
            for (int i = 1; i <= k; i++)
            {
                res *= (n - (k - i));
                res /= i;
            }
            return res;
        }

       public static int[,] CalculateAmEdges(int size, int[] kEdgesArray)
        {
            int[,] AMKEdgesArray = new int[3, size + 1];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j <= size; j++)
                    AMKEdgesArray[i, j] = 0;

            //Enumerable.Repeat(0, size + 1).ToArray();
            AMKEdgesArray[0, 0] = kEdgesArray[0];
            for (int i = 1; i <= size; i++)
            {
                AMKEdgesArray[0, i] = AMKEdgesArray[0, i - 1] + kEdgesArray[i];
            }

            for (int k = 1; k < 3; k++)
            {
                AMKEdgesArray[k, 0] = AMKEdgesArray[k - 1, 0];
                for (int i = 1; i <= size; i++)
                {
                    AMKEdgesArray[k, i] = AMKEdgesArray[k, i - 1] + AMKEdgesArray[k - 1, i];
                }
            }
            return AMKEdgesArray;
        }
    }
}
