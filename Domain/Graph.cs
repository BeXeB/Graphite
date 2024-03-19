namespace Domain
{
    public abstract class Graph<T>
    {
        //tags[n] is an array of the tags of the n-th vertex
        public int noOfVertices;
        public List<List<string>> tags;
        public List<List<T>> adjMatrix;
    }

    public class DGraph : Graph<bool>
    {
        public DGraph()
        {
            noOfVertices = 0;
            tags = [];
            adjMatrix = [];
        }

        public void AddVertex(string[] vertexTags)
        {
            noOfVertices++;
            tags.Add([.. vertexTags]);

            foreach (List<bool> vertex in adjMatrix)
            {
                vertex.Add(false);
            }

            adjMatrix.Add(Enumerable.Repeat(false, noOfVertices).ToList());
        }

        public void AddVertices(List<string[]> vertexTags)
         => vertexTags.ForEach(AddVertex);

        public List<int> GetVertices(Predicate<List<string>> pred)
        {
            List<int> indexes = [];
            int i = 0;
            foreach (List<string> vertex in tags)
            {
                if (pred(vertex))
                {
                    indexes.Add(i);
                }
                i++;
            }
            return indexes;
        }

        public void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred)
        {
            List<int> fromIndexes = GetVertices(fromPred);
            List<int> toIndexes = GetVertices(toPred);
            foreach (int from in fromIndexes)
            {
                foreach (int to in toIndexes)
                {
                    adjMatrix[from][to] = true;
                }
            }
        }

        public void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred)
        {
            List<int> fromIndexes = GetVertices(fromPred);
            List<int> toIndexes = GetVertices(toPred);

            foreach (int from in fromIndexes)
            {
                foreach (int to in toIndexes)
                {
                    adjMatrix[from][to] = false;
                }
            }
        }

        public void Retag(string from, string to)
        {
            List<List<string>> newTags = [];
            int i;
            int j = 0;
            foreach (List<string> vertex in tags)
            {
                newTags.Add([]);
                i = 0;
                foreach (string tag in vertex)
                {
                    if (tag == from)
                    {
                        newTags[j].Add(to);
                    }
                    else
                    {
                        newTags[j].Add(tags[j][i]);
                    }
                    i++;
                }
                j++;
            }
            tags = newTags;
        }
    }
}