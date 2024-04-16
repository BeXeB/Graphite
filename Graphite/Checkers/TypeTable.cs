using Graphite.Lexer;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers
{
    internal class TypeTable
    {
        private Dictionary<string, Type> globalScope = new Dictionary<string, Type>();
        private Stack<Dictionary<string, Type>> scopes = new Stack<Dictionary<string, Type>>();

        public TypeTable()
        {
            globalScope.Add("int", new Type(new Token { type = TokenType.INT, lexeme = "int"}, null));
            globalScope.Add("bool", new Type(new Token { type = TokenType.BOOL, lexeme = "bool"}, null));
            globalScope.Add("str", new Type(new Token { type = TokenType.STR, lexeme = "str"}, null));
            globalScope.Add("char", new Type(new Token { type = TokenType.CHAR, lexeme = "char"}, null));
            globalScope.Add("void", new Type(new Token { type = TokenType.VOID, lexeme = "void"}, null));
        }
        
        public void EnterScope()
        {
            scopes.Push(new Dictionary<string, Type>());
        }

        public void ExitScope()
        {
            scopes.Pop();
        }

        public void AddType(string name, Type type)
        {
            scopes.Peek().Add(name, type);
        }

        public bool IsTypeDeclared(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name))
                    return true;
            }
            return globalScope.ContainsKey(name);
        }
    }
}
