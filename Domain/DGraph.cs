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

        public override void AddGraph(Graph<bool> graphToAdd)
        {
            var oldVertexCount = this.NoOfVertices;
            foreach (var tag in graphToAdd.Tags)
            {
                AddVertex(tag.ToArray());
            }
            for (int i = oldVertexCount; i < (oldVertexCount + graphToAdd.NoOfVertices); i++)
            {
                for (int j = oldVertexCount; j < (oldVertexCount + graphToAdd.NoOfVertices); j++)
                {
                    AdjMatrix[i][j] = graphToAdd.AdjMatrix[i - oldVertexCount][j - oldVertexCount];
                }
            }
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
    }
}
