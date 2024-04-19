using Type = Graphite.Parser.OtherNonTerminals.Type;
using Method = Graphite.Parser.OtherNonTerminals.Method;

namespace Graphite.Checkers
{
    internal class FunctionTable
    {
        private Dictionary<Method, Type> globalScope = [];
        private Stack<Dictionary<Method, Type>> scopes = new();

        public void EnterScope()
        {
            scopes.Push([]);
        }

        public void ExitScope()
        {
            scopes.Pop();
        }

        public void AddFunction(string name, Type funcType, string[] parameterTypes)
        {
            scopes.Peek().Add(new(name, parameterTypes), funcType);
        }

        public bool IsFunctionDeclared(string name, string[] parameterTypes)
        {
            var func = new Method(name, parameterTypes);
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(func))
                    return true;
            }
            return globalScope.ContainsKey(func);
        }

        public bool IsFunctionDeclared(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.Keys.Any(m => m.Name == name))
                    return true;
            }
            return globalScope.Keys.Any(m => m.Name == name);
        }

        public Type GetFunctionType(string name, string[] parameterTypes)
        {
            var func = new Method(name, parameterTypes);
            foreach (var scope in scopes)
            {
                if (scope.TryGetValue(func, out Type? value))
                    return value;
            }
            return globalScope[func];
        }

        public Type GetFunctionType(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.Keys.Any(m => m.Name == name))
                {
                    return scope.First(m => m.Key.Name == name).Value;
                }
            }
            return globalScope.FirstOrDefault(g => g.Key.Name == name).Value;
        }
    }
}
