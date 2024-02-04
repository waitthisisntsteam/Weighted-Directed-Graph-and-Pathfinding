using System;
using System.Collections.Generic;
using System.Drawing;

namespace Weighted_Directed_Graph
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph<Vertex<char>> graph = new Graph<Vertex<char>>();

            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('A')));
            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('B')));
            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('C')));
            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('D')));
            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('E')));
            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('F')));
            graph.AddVertex(new Vertex<Vertex<char>>(new Vertex<char>('G')));

            graph.AddEdge(graph.Search(new Vertex<char>('A')), graph.Search(new Vertex<char>('B')), 1);
            graph.AddEdge(graph.Search(new Vertex<char>('A')), graph.Search(new Vertex<char>('C')), 1);

            graph.AddEdge(graph.Search(new Vertex<char>('B')), graph.Search(new Vertex<char>('E')), 1);
            graph.AddEdge(graph.Search(new Vertex<char>('B')), graph.Search(new Vertex<char>('F')), 1);

            graph.AddEdge(graph.Search(new Vertex<char>('C')), graph.Search(new Vertex<char>('D')), 1);

            graph.AddEdge(graph.Search(new Vertex<char>('E')), graph.Search(new Vertex<char>('F')), 1);

            graph.AddEdge(graph.Search(new Vertex<char>('F')), graph.Search(new Vertex<char>('G')), 1);

            var start = graph.Search(new Vertex<char>('B'));
            var end = graph.Search(new Vertex<char>('B'));

            //Console.WriteLine("Euclidan");
            //List<Vertex<Point>> euclidan = graph.AStar(start, end, Heuristics.Euclidean);
            //foreach (var v in euclidan)
            //{
            //    Console.WriteLine(v.Value);
            //}
            //Console.WriteLine();
            ;
        }
    }
}
