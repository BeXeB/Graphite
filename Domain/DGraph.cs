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

        public override void Retag(string from, string to)
        {
            var newTags = new List<List<string>>();
            var j = 0;

            foreach (var vertex in Tags)
            {
                newTags.Add([]);
                var i = 0;
                foreach (var tag in vertex)
                {
                    if (tag == from)
                    {
                        newTags[j].Add(to);
                    }
                    else
                    {
                        newTags[j].Add(Tags[j][i]);
                    }
                    i++;
                }
                j++;
            }
            Tags = newTags;
        }
    }
}