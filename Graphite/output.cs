using Domain;

class Program
{
    static void Main(string[] args)
    {
        DGraph g = new DGraph();
        int depth = 3;

        void testFunction(DGraph g, string rootTag, int currentDepth)
        {
            if (currentDepth > depth)
            {
                return;
            }

            string leaf1tag = "Leaf" + currentDepth + "1";
            string leaf2tag = "Leaf" + currentDepth + "2";
            g.Retag(rootTag, "Root");
            g.AddVertex([leaf1tag], 1);
            g.AddVertex([leaf2tag], 1);
            g.Connect(v => v.Contains("Root"), v => v.Contains(leaf1tag) || v.Contains(leaf2tag), 1);
            g.Retag("Root", null);
            currentDepth = currentDepth + 1;
            testFunction(g, leaf1tag, currentDepth);
            g.Retag(leaf1tag, null);
            testFunction(g, leaf2tag, currentDepth);
            g.Retag(leaf2tag, null);
        }

        g.AddVertex(["Root"], 1);
        testFunction(g, "Root", 1);
        g.PrintGraphInfo();
    }
}