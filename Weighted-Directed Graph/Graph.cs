using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace Weighted_Directed_Graph
{
    public class Graph<T>
    {
        //setup
        
        private List<Vertex<T>> vertices;

        public IReadOnlyList<Vertex<T>> Vertices => vertices;
        public IReadOnlyList<Edge<T>> Edges { get; set; }

        public int VertexCount => vertices.Count;

        public Graph()
        {
            vertices = new List<Vertex<T>>();
        }
        //functions

        public void AddVertex(Vertex<T> vertex)
        {
            if (vertex != null && vertex.NeighborCount <= 0)
            {
                vertices.Add(vertex);
            }
        }

        public bool RemoveVertex(Vertex<T> vertex)
        {
            bool done = false;
            if (vertices.Contains(vertex))
            {
                for (int i = 0; i < vertex.NeighborCount; i++)
                {
                    done = RemoveEdge(vertex.Neighbors[i].EndingPoint, vertex.Neighbors[i].StartingPoint);
                    done = RemoveEdge(vertex.Neighbors[i].StartingPoint, vertex.Neighbors[i].EndingPoint);
                    if (done == true)
                    {
                        i--;
                    }
                }
                vertices.Remove(vertex);
                return true;
            }
            return false;
        }

        public bool AddEdge(Vertex<T> a, Vertex<T> b, float distance)
        {
            Edge<T> edgeCheck = GetEdge(a, b);
            if (edgeCheck == null)
            {
                Edge<T> AtoBEdge = new Edge<T>(a, b, distance);
                Edge<T> BtoAEdge = new Edge<T>(b, a, distance);
                a.Neighbors.Add(AtoBEdge);
                b.Neighbors.Add(BtoAEdge);

                return true;
            }
            return false;
        }

        public bool AddOneSidedEdge(Vertex<T> a, Vertex<T> b, float distance)
        {
            Edge<T> edgeCheck = GetEdge(a, b);
            if (edgeCheck == null)
            {
                Edge<T> AtoBEdge = new Edge<T>(a, b, distance);
                a.Neighbors.Add(AtoBEdge);

                return true;
            }
            return false;
        }

        public bool RemoveEdge(Vertex<T> a, Vertex<T> b)
        {
            Edge<T> edge1 = GetEdge(a, b);
            if (edge1 != null)
            {
                b.Neighbors.Remove(edge1);
                a.Neighbors.Remove(edge1);
                return true;
            }
            return false;
        }

        public Vertex<T> Search(T value)
        {
            int index = -1;

            for (int i = 0; i < VertexCount; i++)
            {
                if (value.Equals(vertices[i].Value))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return null;
            }
            else
            {
                return vertices[index];
            }
        }

        private Edge<T> GetEdge(Vertex<T> a, Vertex<T> b)
        {
            if (a != null && b != null && a.NeighborCount > 0 && b.NeighborCount > 0)
            {
                for (int i = 0; i < a.NeighborCount; i++)
                {
                    if (a.Neighbors[i].EndingPoint == b)
                    {
                        return a.Neighbors[i];
                    }
                }
            }

            return null;
        }

        //pathfinding

        public List<Vertex<T>> BFS(Vertex<T> start, Vertex<T> end)
        {
            List<Vertex<T>> vertices = new List<Vertex<T>>();
            Queue<Vertex<T>> queue = new Queue<Vertex<T>>();
            Dictionary<Vertex<T>, Vertex<T>> visited = new Dictionary<Vertex<T>, Vertex<T>>();

            Vertex<T> current = start;
            queue.Enqueue(current);
            visited.Add(current, null);

            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                foreach (var v in current.Neighbors)
                {
                    if (!visited.ContainsKey(v.EndingPoint))
                    {
                        queue.Enqueue(v.EndingPoint);
                        visited.Add(v.EndingPoint, v.StartingPoint);
                        if (v.EndingPoint == end)
                        {
                            Vertex<T> runner = end;
                            while (true)
                            {
                                vertices.Add(runner);
                                runner = visited[runner];
                                if (runner == null)
                                {
                                    break;
                                }
                            }

                        }
                    }
                }
            }

            Stack<Vertex<T>> reversed = new Stack<Vertex<T>>();
            foreach (var v in vertices)
            {
                reversed.Push(v);
            }
            vertices.Clear();
            while (reversed.Count > 0)
            {
                vertices.Add(reversed.Pop());
            }

            return vertices;
        }

        public List<Vertex<T>> DFS(Vertex<T> start, Vertex<T> end)
        {
            List<Vertex<T>> vertices = new List<Vertex<T>>();
            Dictionary<Vertex<T>, Vertex<T>> visited = new Dictionary<Vertex<T>, Vertex<T>>();

            visited.Add(start, null);
            DFSHelper(start, end, visited);

            var runner = end;

            while (runner != null)
            {
                vertices.Add(runner);
                runner = visited[runner];
            }
            vertices.Reverse();
            return vertices;
        }
        private bool DFSHelper(Vertex<T> current, Vertex<T> end, Dictionary<Vertex<T>, Vertex<T>> visited)
        {
            if (current == end) return true;

            foreach (var v in current.Neighbors)
            {
                if (visited.ContainsKey(v.EndingPoint))
                {
                    continue;
                }

                visited.Add(v.EndingPoint, current);
                if (DFSHelper(v.EndingPoint, end, visited))
                {
                    return true;
                }
                visited.Remove(v.EndingPoint);
            }

            return false;
        }
        public T WantedValue;
        public T FailValue;
        public List<Vertex<T>> Dijkstra(Vertex<T> start, Vertex<T> end, out Heap<Vertex<T>> priorityQueue)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].distanceFromStart = float.PositiveInfinity;
                Vertices[i].parent = null;
                Vertices[i].visited = false;
                Vertices[i].finalDistance = float.PositiveInfinity;
            }

            
           List<Vertex<T>> vertices = new List<Vertex<T>>();
            priorityQueue = new Heap<Vertex<T>>(VertexCount, Comparer<Vertex<T>>.Create((x, y) => x.distanceFromStart.CompareTo(y.distanceFromStart)));


            start.distanceFromStart = 0;
            priorityQueue.Insert(start);

            Vertex<T> oldVert;
            while (priorityQueue.count > 0)
            {
                Vertex<T> currentVertex = priorityQueue.Pop();
                foreach (var n in currentVertex.Neighbors)
                {
                    float tentativeDistance = currentVertex.distanceFromStart + n.Distance;

                    if (tentativeDistance < n.EndingPoint.distanceFromStart && n.EndingPoint.visited == false)
                    {
                        float previousDistance = n.EndingPoint.distanceFromStart;

                        n.EndingPoint.distanceFromStart = tentativeDistance;
                        n.EndingPoint.parent = currentVertex;
                        n.EndingPoint.visited = false;

                        if (previousDistance == float.PositiveInfinity)
                        {
                            priorityQueue.Insert(n.EndingPoint);
                        }
                    }

                }
                oldVert = currentVertex;
                currentVertex.visited = true;         

                if (end.visited == true)
                {
                    break;
                }
            }

            Vertex<T> runner = end;
            vertices.Add(runner);
            while (runner.parent != null)
            {
                runner = runner.parent;
                vertices.Add(runner);
            }
            vertices.Reverse();

            
            return vertices;
        }

        public List<Vertex<T>> AStar(Vertex<T> start, Vertex<T> end, Func<T, T, double> heuristic, out Heap<Vertex<T>> priorityQueue)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].distanceFromStart = float.PositiveInfinity;
                Vertices[i].parent = null;
                Vertices[i].visited = false;
                Vertices[i].finalDistance = float.PositiveInfinity;
            }

            List<Vertex<T>> vertices = new List<Vertex<T>>();
            priorityQueue = new Heap<Vertex<T>>(VertexCount, Comparer<Vertex<T>>.Create((x, y) => x.finalDistance.CompareTo(y.finalDistance)));

            start.distanceFromStart = 0;
            start.finalDistance = (float)heuristic(start.Value, end.Value);
            priorityQueue.Insert(start);

            while (priorityQueue.count > 0)
            {
                var currentVertex = priorityQueue.Pop();

                foreach (var n in currentVertex.Neighbors)
                {
                    float tentativeDistance = currentVertex.distanceFromStart + n.Distance;
                    
                    if (n.Distance >= 10)
                    {
                        ;
                    }

                    if (tentativeDistance < n.EndingPoint.distanceFromStart)
                    {
                        float previousDistance = n.EndingPoint.distanceFromStart;

                        n.EndingPoint.distanceFromStart = tentativeDistance;
                        n.EndingPoint.parent = currentVertex;
                        n.EndingPoint.visited = false;
                        n.EndingPoint.finalDistance = tentativeDistance + (float)heuristic(end.Value, n.EndingPoint.Value);
                    }


                    if (!priorityQueue.Contains(n.EndingPoint) && !n.EndingPoint.visited)
                    {
                        priorityQueue.Insert(n.EndingPoint);
                    }
                }

                currentVertex.visited = true;

                if (end.visited == true)
                {
                    break;
                }
            }

            Vertex<T> runner = end;
            vertices.Add(end);
            while (runner.parent != null)
            {
                runner = runner.parent;
                vertices.Add(runner);
            }
            vertices.Reverse();
            return vertices;
        }

        public bool BellmanFord()
        {
            Vertex<T> currentVertex = null;

            for (int i = 0; i < VertexCount; i++)
            {
                Vertices[i].distanceFromStart = float.PositiveInfinity;
                Vertices[i].parent = null;
                Vertices[i].visited = false;
            }

            currentVertex = Vertices[0];
            currentVertex.distanceFromStart = 0;


            for (int i = 0; i < VertexCount - 1; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    currentVertex = Vertices[j];
                    foreach (var n in currentVertex.Neighbors)
                    {
                        float tentativeDistance = currentVertex.distanceFromStart + n.Distance;
                        if (tentativeDistance < n.EndingPoint.distanceFromStart)
                        {
                            n.EndingPoint.distanceFromStart = tentativeDistance;
                            n.EndingPoint.parent = currentVertex;
                        }
                    }
                }
            }

            for (int j = 0; j < VertexCount; j++)
            {
                currentVertex = Vertices[j];
                foreach (var n in currentVertex.Neighbors)
                {
                    if (n.EndingPoint != currentVertex)
                    {
                        float tentativeDistance = currentVertex.distanceFromStart + n.Distance;

                        if (tentativeDistance < n.EndingPoint.distanceFromStart)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
