namespace Domain
{
    public abstract class Graph<T> : IGraph
    {
        protected int NoOfVertices = 0;
        protected List<List<string>> Tags = []; //tags[n] is an array of the tags of the n-th vertex
        protected List<List<T>> AdjMatrix = [];

        public abstract void AddVertex(string[] vertexTags);
        public abstract void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public abstract void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public abstract void AddTags(Predicate<List<string>> pred, List<string> tags);
        public abstract void RemoveTags(Predicate<List<string>> pred, List<string> tags);
        public abstract void Retag(string from, string to);

        public void AddVertices(List<string[]> vertexTags) => vertexTags.ForEach(AddVertex);
        public int[] GetVertices(Predicate<List<string>> pred)
            => Enumerable.Range(0, Tags.Count).Where(i => pred(Tags[i])).ToArray();

        public void PrintGraphInfo()
        {
            Console.WriteLine("Graph Information and Adjacency Matrix: " + GetType().Name);
            Console.WriteLine("+-----------+-------------------------------+------------------+");
            Console.WriteLine("| Vertex    | Tags                          | Adjacency Matrix |");
            Console.WriteLine("+-----------+-------------------------------+------------------+");

            for (int i = 0; i < NoOfVertices; i++)
            {
                string vertex = $"| {i + 1,-9} | [{string.Join(", ", Tags[i])}]";

                string adjacencyMatrixRow = "| ";
                for (int j = 0; j < NoOfVertices; j++)
                {
                    adjacencyMatrixRow += $"{(AdjMatrix[i][j].ToString() == "True" ? "T" : "-"),-4} ";
                }
                adjacencyMatrixRow += "|";

                Console.WriteLine($"{vertex,-44}{adjacencyMatrixRow,-17}");
            }

            Console.WriteLine("+-----------+-------------------------------+-----------------+");
        }
    }

    public interface IGraph
    {
        public void AddVertex(string[] vertexTags);
        public void AddVertices(List<string[]> vertexTags);
        public int[] GetVertices(Predicate<List<string>> pred);
        public void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public void AddTags(Predicate<List<string>> pred, List<string> tags);
        public void RemoveTags(Predicate<List<string>> pred, List<string> tags);
        public void Retag(string from, string to);
    }
}