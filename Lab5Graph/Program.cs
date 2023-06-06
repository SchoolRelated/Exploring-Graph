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

        public static int Diameter(UndirectedGraph<int, Edge<int>> graph)
        {
            int diameter = 0;
            foreach (var source in graph.Vertices)
            {
                var distances = new Dictionary<int, int>();
                var queue = new Queue<int>();
                queue.Enqueue(source);
                distances[source] = 0;
                while (queue.Count > 0)
                {
                    int vertex = queue.Dequeue();
                    foreach (var edge in graph.AdjacentEdges(vertex))
                    {
                        int neighbor = edge.Target;
                        if (!distances.ContainsKey(neighbor))
                        {
                            distances[neighbor] = distances[vertex] + 1;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                int maxDistance = distances.Values.Max();
                if (maxDistance > diameter)
                {
                    diameter = maxDistance;
                }
            }
            return diameter;
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

        public static double ClusteringCoefficient(UndirectedGraph<int, Edge<int>> graph)
        {
            double clusteringCoefficient = 0;
            int count = 0;
            object lockObject = new object();
            Parallel.ForEach(graph.Vertices, vertex =>
            {
                int degree = graph.AdjacentDegree(vertex);
                if (degree > 1)
                {
                    int edgesBetweenNeighbors = 0;
                    var neighbors = graph.AdjacentEdges(vertex).Select(edge => edge.Target).ToList();
                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        for (int j = i + 1; j < neighbors.Count; j++)
                        {
                            if (graph.ContainsEdge(neighbors[i], neighbors[j]))
                            {
                                edgesBetweenNeighbors++;
                            }
                        }
                    }
                    double vertexClusteringCoefficient = (double)(2 * edgesBetweenNeighbors) / (degree * (degree - 1));
                    lock (lockObject)
                    {
                        clusteringCoefficient += vertexClusteringCoefficient;
                        count++;
                    }
                }
            });
            if (count > 0)
            {
                clusteringCoefficient /= count;
            }
            return clusteringCoefficient;
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

        static void printInfo(UndirectedGraph<int, Edge<int>> graph)
        {
            CalcNodesEdges(graph);
            double averageDegree = CalcAverageDegree(graph);
            Console.WriteLine($"AverageDegree: {averageDegree}");
            double Density = CalcDensity(graph);
            Console.WriteLine($"Density: {Density}");
            int diameter = Diameter(graph);
            Console.WriteLine($"Diameter is: {diameter}");
            double clusteringCoefficient = ClusteringCoefficient(graph);
            Console.WriteLine($"Clustering Coefficient is: {clusteringCoefficient}");
            Console.WriteLine("");
        }

        public static Dictionary<int, int> DegreeDistribution(UndirectedGraph<int, Edge<int>> graph)
        {
            var degreeDistribution = new Dictionary<int, int>();
            foreach (var vertex in graph.Vertices)
            {
                int degree = graph.AdjacentDegree(vertex);
                if (!degreeDistribution.ContainsKey(degree))
                {
                    degreeDistribution[degree] = 0;
                }
                degreeDistribution[degree]++;
            }
            return degreeDistribution;
        }


        static void Main(string[] args)
        {
            Console.WriteLine("For first graph");
            string filePath = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\ENZYMES_g325.edges";
            string filePath1 = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\\\over5000\\circuit_2\\circuit_2.mtx";
            string filePath2 = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\\\over5000\\mark3jac140\\mark3jac140.mtx";
            string filePath3 = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\\\over5000\\mk10-b3\\mk10-b3.mtx";
            string filePath4 = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\\\over5000\\mk10-b4\\mk10-b4.mtx";
            string filePath5 = "D:\\Study stuffs\\3rd Year\\6th Sem\\Time Complexity and Algorithm\\Lab Works\\Lab5Graph\\Lab5Graph\\Graphs\\\\over5000\\msc04515\\msc04515.mtx";

            //string currentDirectory = Directory.GetCurrentDirectory();
            //Console.WriteLine("Current directory: " + currentDirectory);
            var graph = ReadFromGraphFile(filePath);
            var graph1 = ReadFromGraphFile(filePath1);
            var graph2 = ReadFromGraphFile(filePath2);
            var graph3 = ReadFromGraphFile(filePath3);
            var graph4 = ReadFromGraphFile(filePath4);
            var graph5 = ReadFromGraphFile(filePath5);
            //printInfo(graph);
            DegreeDistribution(graph);

            Console.WriteLine("For Graphs with about 5000 nodes");
            //printInfo(graph1);
            //printInfo(graph2);
            //printInfo(graph3);
            //printInfo(graph4);
            //printInfo(graph5);
            var degreeDistribution = DegreeDistribution(graph);
            foreach (var pair in degreeDistribution)
            {
                int degree = pair.Key;
                int count = pair.Value;
                Console.WriteLine($"Degree: {degree}, Count: {count}");
            }

            Console.WriteLine(" ");
            var degreeDistribution1 = DegreeDistribution(graph1);
            foreach (var pair in degreeDistribution1)
            {
                int degree = pair.Key;
                int count = pair.Value;
                Console.WriteLine($"Degree: {degree}, Count: {count}");
            }

            Console.WriteLine(" ");
            var degreeDistribution2 = DegreeDistribution(graph2);
            foreach (var pair in degreeDistribution2)
            {
                int degree = pair.Key;
                int count = pair.Value;
                Console.WriteLine($"Degree: {degree}, Count: {count}");
            }

            Console.WriteLine(" ");
            var degreeDistribution3 = DegreeDistribution(graph3);
            foreach (var pair in degreeDistribution3)
            {
                int degree = pair.Key;
                int count = pair.Value;
                Console.WriteLine($"Degree: {degree}, Count: {count}");
            }

            Console.WriteLine(" ");
            var degreeDistribution4 = DegreeDistribution(graph4);
            foreach (var pair in degreeDistribution4)
            {
                int degree = pair.Key;
                int count = pair.Value;
                Console.WriteLine($"Degree: {degree}, Count: {count}");
            }

            Console.WriteLine(" ");
            var degreeDistribution5 = DegreeDistribution(graph5);
            foreach (var pair in degreeDistribution5)
            {
                int degree = pair.Key;
                int count = pair.Value;
                Console.WriteLine($"Degree: {degree}, Count: {count}");
            }


        }
    }

}