﻿using Graphite.Parser;
using static Graphite.Checkers.VariableTable;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers
{
    internal class FunctionTable
    {
        private Dictionary<string, Function> globalScope = new Dictionary<string, Function>();
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
            return globalScope.ContainsKey(name);
        }

        public Type GetFunctionType(string name)
        {
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name))
                    return scope[name].ReturnType;
            }
            return globalScope[name].ReturnType;
        }
    }
}
