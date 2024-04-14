﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphite.Checkers
{
    internal class FunctionTable
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

        public void AddFunction(string name, Type type)
        {
            scopes.Peek().Add(name, type);
        }

        public bool IsFunctionDeclared(string name)
        {
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
                    return scope[name];
            }
            return globalScope[name];
        }
    }
}