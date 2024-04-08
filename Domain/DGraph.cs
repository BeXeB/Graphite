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
        public override void AddVertex(string[] vertexTags, int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                NoOfVertices++;
                Tags.Add([.. vertexTags]);

                foreach (var vertex in AdjMatrix)
                {
                    vertex.Add(false);
                }

                AdjMatrix.Add(Enumerable.Repeat(false, NoOfVertices).ToList());
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
        
        public override void RemoveVertex(Predicate<List<string>> pred)
        {
            //TODO: Test this method
            var indexes = GetVertices(pred);

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
    }
}
