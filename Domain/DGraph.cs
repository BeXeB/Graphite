namespace Domain
{
    public class DGraph : Graph<bool>
    {
        public override void AddVertex(string[] vertexTags)
        {
            NoOfVertices++;
            Tags.Add([.. vertexTags]);

            foreach (var vertex in AdjMatrix)
            {
                vertex.Add(false);
            }

            AdjMatrix.Add(Enumerable.Repeat(false, NoOfVertices).ToList());
        }

        public override void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred)
        {
            var fromIndexes = GetVertices(fromPred);
            var toIndexes = GetVertices(toPred);

            foreach (var from in fromIndexes)
            {
                foreach (var to in toIndexes)
                {
                    AdjMatrix[from][to] = true;
                }
            }
        }

        public override void Disconnect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred)
        {
            var fromIndexes = GetVertices(fromPred);
            var toIndexes = GetVertices(toPred);

            foreach (var from in fromIndexes)
            {
                foreach (var to in toIndexes)
                {
                    AdjMatrix[from][to] = false;
                }
            }
        }

        public override void AddTags(Predicate<List<string>> pred, List<string> tags)
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

        public override void RemoveTags(Predicate<List<string>> pred, List<string> tags)
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

        public override void Retag(string from, string to)
        {
            var newTags = new List<List<string>>();

            foreach (var vertex in Tags)
            {
                var newVertex = new List<string>();
                foreach (var tag in vertex)
                {
                    if (tag == from)
                    {
                        newVertex.Add(to);
                    }
                    else
                    {
                        newVertex.Add(tag);
                    }
                }
                newTags.Add(newVertex);
            }
            Tags = newTags;
        }
    }
}
