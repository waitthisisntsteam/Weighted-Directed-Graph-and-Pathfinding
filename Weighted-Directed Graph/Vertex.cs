using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Weighted_Directed_Graph
{
    [DebuggerDisplay("Val: {Value}")]
    public class Vertex<T> : IComparable<Vertex<T>>
    {
        //for Dijkstra
        public float distanceFromStart;
        public Vertex<T> parent;
        public bool visited;

        //for A*
        public float finalDistance;

        //always needed
        public T Value { get; set; }
        public List<Edge<T>> Neighbors { get; set; }

        public int NeighborCount => Neighbors.Count;

        public Vertex(T value)
        {
            Neighbors = new List<Edge<T>>();
            Value = value;

            //for Dijkstra
            distanceFromStart = 0;
            parent = null;
            visited = false;

            //for A*
            finalDistance = float.PositiveInfinity;
        }

        public int CompareTo([AllowNull] Vertex<T> other)
        {
            if (other.distanceFromStart > distanceFromStart)
            {
                return -1;
            }
            if (other.distanceFromStart == distanceFromStart)
            {
                return 0;
            }
            return 1;

        }
    }
}
