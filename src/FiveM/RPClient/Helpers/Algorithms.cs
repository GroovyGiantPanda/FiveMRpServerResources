using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Helpers
{
    class PolygonCollision
    {
        public static bool Intersects(float[] A, float[] B, float[] P)
        {
            if (A[1] > B[1])
                return Intersects(B, A, P);

            if (P[1] == A[1] || P[1] == B[1])
                P[1] += 0.0001f;

            if (P[1] > B[1] || P[1] < A[1] || P[0] > Math.Max(A[0], B[00]))
                return false;

            if (P[0] < Math.Min(A[0], B[0]))
                return true;

            double red = (P[1] - A[1]) / (double)(P[0] - A[0]);
            double blue = (B[1] - A[1]) / (double)(B[0] - A[0]);
            return red >= blue;
        }

        public static bool Contains(float[][] shape, float[] pnt)
        {
            bool inside = false;
            int len = shape.Length;
            for(int i = 0; i < len; i++)
            {
                if (Intersects(shape[i], shape[(1 + i) % len], pnt))
                    inside = !inside;
            }
            return inside;
        }

        //public static void Test()
        //{
        //    double[][] testPoints = {new double[] {10, 10}, new double[] {10, 16}, new double[] {-20, 10}, new double[] {0, 10},
        //new double[] {20, 10}, new double[] {16, 10}, new double[] {20, 20}};

        //    foreach (int[][] shape in shapes)
        //    {
        //        foreach (double[] pnt in testPoints)
        //            Log.ToChat(Contains(shape, pnt).ToString());
        //    }
        //}

        //static int[][] square = { new int[] { 0, 0 }, new int[] { 20, 0 }, new int[] { 20, 20 }, new int[] { 0, 20 } };

        //static int[][] squareHole = { new int[] {0, 0},  new int[] {20, 0}, new int[] {20, 20},  new int[] {0, 20},
        //     new int[] {5, 5},  new int[] {15, 5},  new int[] {15, 15},  new int[] {5, 15}};

        //static int[][] strange = { new int[] {0, 0},  new int[] {5, 5},  new int[] {0, 20},  new int[] {5, 15}, new int[] {15, 15},
        //    new int[] {20, 20}, new int[] {20, 0}};

        //static int[][] hexagon = {new int[] {6, 0}, new int[] {14, 0}, new int[] {20, 10}, new int[] {14, 20},
        //    new int[] {6, 20}, new int[] {0, 10}};

        //static int[][][] shapes = { square, squareHole, strange, hexagon };
    }
}