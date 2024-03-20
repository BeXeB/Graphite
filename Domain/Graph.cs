namespace Domain
{
    public abstract class Graph<T> : IGraph
    {
        protected int NoOfVertices;
        protected List<List<string>> Tags; //tags[n] is an array of the tags of the n-th vertex
        protected List<List<T>> AdjMatrix;

        public abstract void AddVertex(string[] vertexTags);
        public abstract void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public abstract void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public abstract void Retag(string from, string to);

        public void AddVertices(List<string[]> vertexTags) => vertexTags.ForEach(AddVertex);
        public int[] GetVertices(Predicate<List<string>> pred)
            => Enumerable.Range(0, Tags.Count).Where(i => pred(Tags[i])).ToArray();
    }

    public interface IGraph
    {
        public void AddVertex(string[] vertexTags);
        public void AddVertices(List<string[]> vertexTags);
        public int[] GetVertices(Predicate<List<string>> pred);
        public void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred);
        public void Retag(string from, string to);
    }
}