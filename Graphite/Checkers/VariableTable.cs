using Graphite.Lexer;
using Graphite.Parser;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers
{
    internal class VariableTable
    {
        private Stack<Dictionary<string, Variable>> scopes = new Stack<Dictionary<string, Variable>>();

        public record Variable
        {
            public string Name;
            public OtherNonTerminals.Type Type;
            public bool IsInitialized;
        }

        public void EnterScope()
        {
            scopes.Push(new Dictionary<string, Variable>());
        }

        public void ExitScope()
        {
            scopes.Pop();
        }

        public void AddVariable(string name, Variable variable)
        {
            scopes.Peek().Add(name, variable);
        }

        public bool IsVariableDeclared(string name, bool inCurrentScope = false)
        {
            if (inCurrentScope)
                return scopes.Peek().ContainsKey(name);
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name))
                    return true;
            }

            return false;
        }

        public Type? GetVariableType(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name))
                    return scope[name].Type;
            }

            return null;
        }
    }
}
