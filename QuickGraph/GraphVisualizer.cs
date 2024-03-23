using Domain;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System.Diagnostics;

namespace QuikGraphVisualizer
{
    public static class GraphVisualizer<T>
    {
        public static void VisualizeInBrowser(Graph<T> graph)
        {
            var quikGraph = ToQuikGraph(graph);
            var graphviz = new GraphvizAlgorithm<string, Edge<string>>(quikGraph);

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

            var baseUrl = "https://dreampuf.github.io/GraphvizOnline/#";

            var url = $"{baseUrl}{Uri.EscapeDataString(dotGraph)}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private static BidirectionalGraph<string, Edge<string>> ToQuikGraph(Graph<T> graph)
        {
            var quikGraph = new BidirectionalGraph<string, Edge<string>>();

            // Add vertices
            for (int i = 0; i < graph.NoOfVertices; i++)
            {
                quikGraph.AddVertex($"Vertex {i}");
            }

            // Add edges
            for (int i = 0; i < graph.NoOfVertices; i++)
            {
                for (int j = 0; j < graph.NoOfVertices; j++)
                {
                    if (!graph.AdjMatrix[i][j].Equals(default(T))) // Assuming default(T) means no edge
                    {
                        quikGraph.AddEdge(new Edge<string>($"Vertex {i}", $"Vertex {j}"));
                    }
                }
            }

            return quikGraph;
        }
    }
}