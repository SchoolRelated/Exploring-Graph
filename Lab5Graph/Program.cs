using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;



namespace Graph
{
    class Program
    {
        static UndirectedGraph<int, Edge<int>> ReadFromGraphFile(String filePath) //BidirectionalGraph<int, Edge<int>> for directed
        {
            var graph = new UndirectedGraph<int, Edge<int>>();
            string fileContent = File.ReadAllText(filePath);


            var lines = fileContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //bool firstLine = true;
            foreach (var line in lines)
            {
                //if(firstLine)
                //{
                //    //as the first line contains metadata
                //    firstLine = false;
                //    continue;
                //}

                if (line.StartsWith("%"))
                {
                    // Skip comment lines starting with '%'
                    continue;
                }

                var parts = line.Split(' ');
                if (parts.Length >= 2)
                {
                    int source = int.Parse(parts[0]);
                    int target = int.Parse(parts[1]);

                    graph.AddVertex(source);
                    graph.AddVertex(target);
                    graph.AddEdge(new Edge<int>(source, target));
                }
            }
            return graph;
        }

        public static void CalcNodesEdges(UndirectedGraph<int, Edge<int>> graph)
        {
            int nodeCount = graph.VertexCount;
            int edgeCount = graph.EdgeCount;

            Console.WriteLine("The number of Nodes: " + nodeCount);
            Console.WriteLine("The number of Edges: " + edgeCount);
        }

        public static double CalcAverageDegree(UndirectedGraph<int, Edge<int>> graph)
        {
            double totalDegree = graph.Vertices.Sum(vertex => graph.AdjacentDegree(vertex));
            double averageDegree = totalDegree / (2 * graph.VertexCount);

            return averageDegree;
        }

        public static double CalcDensity(UndirectedGraph<int, Edge<int>> graph)
        {
            if (graph.VertexCount > 1)
            {
                double density = 2.0 * graph.EdgeCount / (graph.VertexCount * (graph.VertexCount - 1));
                return density;
            }

            else
            {
                return 0;
            }

        }

        static void VisualizeGraph(UndirectedGraph<int, Edge<int>> graph)
        {
            // Create a new GraphvizAlgorithm instance
            var graphviz = new GraphvizAlgorithm<int, Edge<int>>(graph);

            // Set some properties of the GraphvizAlgorithm instance
            graphviz.CommonVertexFormat.Font = new GraphvizFont("SansSerif", 12); 
            graphviz.CommonVertexFormat.Style = GraphvizVertexStyle.Filled;
            graphviz.CommonVertexFormat.FillColor = new GraphvizColor(192, 192, 192, 1);
            graphviz.FormatVertex += (sender, args) => args.VertexFormatter.Label = args.Vertex.ToString();
            graphviz.GraphFormat.RankDirection = GraphvizRankDirection.LR;


            // Generate the DOT file
            string dotFile = graphviz.Generate();

            // Save the DOT file to disk
            File.WriteAllText("graph.dot", dotFile);

            // Render the DOT file using Graphviz
            var renderer = new Process();
            renderer.StartInfo.FileName = @"C:\Program Files (x86)\Graphviz\bin\dot.exe";
            renderer.StartInfo.Arguments = "-Tpng graph.dot -o graph.png";
            renderer.Start();
            renderer.WaitForExit();
        }


        static void Main(string[] args)
        {
            string filePath = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\ENZYMES_g325.edges";

            //string currentDirectory = Directory.GetCurrentDirectory();
            //Console.WriteLine("Current directory: " + currentDirectory);

            
            var graph = ReadFromGraphFile(filePath);

            CalcNodesEdges(graph);
            
            double averageDegree = CalcAverageDegree(graph);
            Console.WriteLine($"AverageDegree: {averageDegree}");

            double Density = CalcDensity(graph);
            Console.WriteLine($"Density: {Density}");

            //VisualizeGraph(graph);

        }
    }

}