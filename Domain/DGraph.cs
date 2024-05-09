namespace Domain
{
    public class DGraph : Graph<bool>
    {
        public override void AddVertex(string[] vertexTags)
        {
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
        
        public override void Connect(Predicate<List<string>> fromPred, Predicate<List<string>> toPred, object weight)
        {
            var fromIndexes = GetVertices(fromPred);
            var toIndexes = GetVertices(toPred);
            
            var weightBool = Convert.ToBoolean(weight);
            
            foreach (var from in fromIndexes)
            {
                foreach (var to in toIndexes)
                {
                    AdjMatrix[from][to] = weightBool;
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
