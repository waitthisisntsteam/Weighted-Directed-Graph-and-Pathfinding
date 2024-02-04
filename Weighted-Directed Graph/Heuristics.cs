using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Weighted_Directed_Graph
{
    public class Heuristics
    {

        public static double Manhattan(Point start, Point end)
        {
            float dx = Math.Abs(start.X - end.X);
            float dy = Math.Abs(start.Y - end.Y);
            return 1 * (dx + dy);
        }

        private static double Diagonal(Point start, Point end, double D, double D2)
        {
            float dx = Math.Abs(start.X - end.X);
            float dy = Math.Abs(start.Y - end.Y);
            return D * (dx + dy) + (D2 - 2 * D) * Math.Min(dx, dy);
        }

        public static double Chebyshev(Point start, Point end)
        {
            return Diagonal(start, end, 1, 1);
        }

        public static double Octile(Point start, Point end)
        {
            return Diagonal(start, end, 1, Math.Sqrt(2));
        }

        
        public static double Euclidean(Point start, Point end)
        {
            float dx = Math.Abs(start.X - end.X);
            float dy = Math.Abs(start.Y - end.Y);
            return 1 * Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
