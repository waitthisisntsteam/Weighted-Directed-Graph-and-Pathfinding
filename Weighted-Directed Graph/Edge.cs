using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Weighted_Directed_Graph
{
    [DebuggerDisplay("EPV: {EndingPoint}")]
    public class Edge<T>
    {
        public Vertex<T> StartingPoint { get; set; }
        public Vertex<T> EndingPoint { get; set; }
        public float Distance { get; set; }

        public Edge(Vertex<T> startingpoint, Vertex<T> endingpoint, float distance)
        {
            StartingPoint = startingpoint;
            EndingPoint = endingpoint;
            Distance = distance;
        }
    }
}
