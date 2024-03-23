using Domain;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System.Diagnostics;

namespace QuikGraphVisualizer
{
    public static class GraphVisualizer<T>
    {
        private const string GraphvizOnlineBaseUrl = "https://dreampuf.github.io/GraphvizOnline/#";

        public static void VisualizeInBrowser(Graph<T> graph)
        {
            var graphviz = new GraphvizAlgorithm<string, Edge<string>>(graph is DGraph ? ToQuikDGraph(graph) : ToQuikUGraph(graph));

            // Customize vertex appearance
            graphviz.FormatVertex += (sender, args) =>
            {
                args.VertexFormat.Label = args.Vertex;
                args.VertexFormat.Shape = GraphvizVertexShape.Circle;
                args.VertexFormat.Style = GraphvizVertexStyle.Filled;
                args.VertexFormat.FillColor = GraphvizColor.LightSkyBlue;
                args.VertexFormat.Font = new GraphvizFont("Arial", 12);
            };

            // Customize edge appearance
            graphviz.FormatEdge += (sender, args) =>
            {
                args.EdgeFormat.Label.Value = " ";
                args.EdgeFormat.Style = GraphvizEdgeStyle.Solid;
                args.EdgeFormat.Font = new GraphvizFont("Arial", 10);
                args.EdgeFormat.FontColor = GraphvizColor.Black;
            };

            var dotGraph = graphviz.Generate();

            var url = $"{GraphvizOnlineBaseUrl}{Uri.EscapeDataString(dotGraph)}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private static BidirectionalGraph<string, Edge<string>> ToQuikDGraph(Graph<T> graph)
        {
            var quikGraph = new BidirectionalGraph<string, Edge<string>>();

            // Add vertices
            for (int i = 0; i < graph.NoOfVertices; i++)
            {
                quikGraph.AddVertex(string.Join(", ", graph.Tags[i]));
            }

            // Add edges
            for (int i = 0; i < graph.NoOfVertices; i++)
            {
                for (int j = 0; j < graph.NoOfVertices; j++)
                {
                    if (!graph.AdjMatrix[i][j].Equals(default(T))) // Assuming default(T) means no edge
                    {
                        quikGraph.AddEdge(new Edge<string>(string.Join(", ", graph.Tags[i]), string.Join(", ", graph.Tags[j])));
                    }
                }
            }

            return quikGraph;
        }

        private static UndirectedGraph<string, Edge<string>> ToQuikUGraph(Graph<T> graph)
        {
            var quikGraph = new UndirectedGraph<string, Edge<string>>();

            // Add vertices
            for (int i = 0; i < graph.NoOfVertices; i++)
            {
                quikGraph.AddVertex(string.Join(", ", graph.Tags[i]));
            }

            // Add edges
            for (int i = 0; i < graph.NoOfVertices; i++)
            {
                for (int j = 0; j < graph.NoOfVertices; j++)
                {
                    if (!graph.AdjMatrix[i][j].Equals(default(T))) // Assuming default(T) means no edge
                    {
                        if (quikGraph.TryGetEdge(string.Join(", ", graph.Tags[j]), string.Join(", ", graph.Tags[i]), out _))
                            continue;
                        quikGraph.AddEdge(new Edge<string>(string.Join(", ", graph.Tags[i]), string.Join(", ", graph.Tags[j])));
                    }
                }
            }

            return quikGraph;
        }
    }
}