using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers
{
    internal class VariableTable
    {
        private Dictionary<string, Type> globalScope = new Dictionary<string, Type>();
        private Stack<Dictionary<string, Type>> scopes = new Stack<Dictionary<string, Type>>();

        public void EnterScope()
        {
            scopes.Push(new Dictionary<string, Type>());
        }

        public void ExitScope()
        {
            scopes.Pop();
        }

        public void AddVariable(string name, Type type)
        {
            scopes.Peek().Add(name, type);
        }

        public bool IsVariableDeclared(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name))
                    return true;
            }
            return globalScope.ContainsKey(name);
        }

        public Type GetVariableType(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name))
                    return scope[name];
            }
            return globalScope[name];
        }
    }
}
