using Graphite.Lexer;
using Graphite.Parser;
using static Graphite.Checkers.VariableTable;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers
{
    internal class FunctionTable
    {
        private Stack<Dictionary<string, Function>> scopes = new Stack<Dictionary<string, Function>>();

        public class Function
        {
            public string Name;
            public List<Variable> Parameters;
            public OtherNonTerminals.Type ReturnType;
        }

        public void EnterScope()
        {
            scopes.Push(new Dictionary<string, Function>());
        }

        public void ExitScope()
        {
            scopes.Pop();
        }

        public void AddFunction(string name, Function function)
        {
            scopes.Peek().Add(name, function);
        }

        public bool IsFunctionDeclared(string name, bool inCurrentScope = false)
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

        public Type? GetFunctionType(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.TryGetValue(name, out var value))
                    return CreateFunctionType(value);
            }

            return null;
        }

        private Type CreateFunctionType(Function func)
        {
            List<Type> typeArguments = [func.ReturnType];
            typeArguments.AddRange(func.Parameters.Select(parameter => parameter.Type).ToList());
            return new Type(
                new Token { type = TokenType.FUNC_TYPE, lexeme = "Func" },
                typeArguments
            );
        }
    }
}