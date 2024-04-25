namespace Domain
{
    public abstract class Graph<T> : IGraph
    {
        public int NoOfVertices => Tags.Count;
        public List<List<string?>> Tags { get; protected set; } = []; //tags[n] is an array of the tags of the n-th vertex
        public List<List<T>> AdjMatrix { get; } = [];

        public abstract void AddVertex(string[] vertexTags);
        public abstract void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public abstract void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred, object weight);
        public abstract void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public void AddVertices(List<string[]> vertexTags) => vertexTags.ForEach(AddVertex);

        public int[] GetVertices(Predicate<List<string>> pred)
            => Enumerable.Range(0, Tags.Count).Where(i => pred(Tags[i])).ToArray();

        public void AddVertex(string[] vertexTags, int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                AddVertex(vertexTags);
            }
        }

        public void Retag(string from, string? to)
        {
            var newTags = new List<List<string>>();

            if (to is null)
            {
                foreach (var vertex in Tags)
                {
                    vertex.RemoveAll(t => t == from);
                }
            }

            foreach (var vertex in Tags)
            {
                var newVertex = new List<string>();
                foreach (var tag in vertex)
                {
                    if (tag == from)
                    {
                        newVertex.Add(to!);
                    }
                    else
                    {
                        newVertex.Add(tag!);
                    }
                }
                newTags.Add(newVertex);
            }
            Tags = newTags;
        }

        public void AddTags(Predicate<List<string>> pred, List<string?> tags)
        {
            var indexes = GetVertices(pred);

            foreach (var index in indexes)
            {
                foreach (var tag in tags)
                {
                    if (!Tags[index].Contains(tag))
                    {
                        Tags[index].Add(tag);
                    }
                }
            }
        }

        public void RemoveTags(Predicate<List<string>> pred, List<string> tags)
        {
            var indexes = GetVertices(pred);

            foreach (var index in indexes)
            {
                foreach (var tag in tags)
                {
                    Tags[index].RemoveAll(t => t == tag);
                }
            }
        }

        public void RemoveVertex(Predicate<List<string>> pred)
        {
            var indexes = GetVertices(pred);
            //Reverse so the higher indexes are removed first, so the lower indexes don't change
            indexes = indexes.Reverse().ToArray(); 
            
            foreach (var index in indexes)
            {
                Tags.RemoveAt(index);
                AdjMatrix.RemoveAt(index);

                foreach (var vertex in AdjMatrix)
                {
                    vertex.RemoveAt(index);
                }
            }
        }

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
        public void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred, object weight);
        public void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public void RemoveVertex(Predicate<List<string>> pred);
        public void AddTags(Predicate<List<string>> pred, List<string?> tags);
        public void RemoveTags(Predicate<List<string>> pred, List<string> tags);
        public void Retag(string from, string? to);
    }
}