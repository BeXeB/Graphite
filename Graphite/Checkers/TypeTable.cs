using Graphite.Lexer;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers;

internal class TypeTable
{
    private Dictionary<string, Type> globalScope = new Dictionary<string, Type>();

    public TypeTable()
    {
        //Graph
        var graphFields = new Dictionary<string, Type>
        {
            {
                "NumberOfVertices",
                new Type(new Token { type = TokenType.INT, lexeme = "int" }, null)
            },
            {
                "Tags",
                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                            })
                    })
            },
            {
                "AdjMatrix",
                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.DEC, lexeme = "dec" }, null)
                            })
                    })
            }
        };
        var graphMethods = new Dictionary<string, Type>
        {
            //Func<void, Set<str>>
            {
                "AddVertex",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.SET, lexeme = "Set" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                            })
                    })
            },
            //Func<void, Func<bool, List<str>>, Func<bool, List<str>>>
            {
                "Connect",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            }),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            })
                    })
            },
            //Func<void, Func<bool, List<str>>, Func<bool, List<str>>>
            {
                "Disconnect",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            }),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            })
                    })
            },
            //Func<void, List<Set<str>>>
            {
                "AddVertices",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.SET, lexeme = "Set" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            })
                    })
            },
            //Func<Set<int>, Func<bool, List<str>>>
            {
                "GetVertices",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.SET, lexeme = "Set" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.INT, lexeme = "int" }, null)
                            }),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            })
                    })
            },
            //Func<void, str, str>
            {
                "Retag",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null),
                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                    })
            },
            //Func<void, Func<bool, List<str>>, Set<str>>
            {
                "AddTags",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            }),
                        new Type(new Token { type = TokenType.SET, lexeme = "Set" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                            })
                    })
            },
            //Func<void, Func<bool, List<str>>, Set<str>>
            {
                "RemoveTags",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null),
                        new Type(new Token { type = TokenType.FUNC_TYPE },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.BOOL, lexeme = "bool" }, null),
                                new Type(new Token { type = TokenType.LIST, lexeme = "List" },
                                    new List<Type>
                                    {
                                        new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                                    })
                            }),
                        new Type(new Token { type = TokenType.SET, lexeme = "Set" },
                            new List<Type>
                            {
                                new Type(new Token { type = TokenType.STR, lexeme = "string" }, null)
                            })
                    })
            },
            //Func<void>
            {
                "PrintGraphInfo",
                new Type(new Token { type = TokenType.FUNC_TYPE },
                    new List<Type>
                    {
                        new Type(new Token { type = TokenType.VOID, lexeme = "void" }, null)
                    })
            }
        };
        var graphType = new Type(new Token { type = TokenType.IDENTIFIER, lexeme = "Graph" }, null);
        graphFields.ToList().ForEach(f => graphType.AddField((f.Key, f.Value)));
        graphMethods.ToList().ForEach(m => graphType.AddMethod((m.Key, m.Value)));
        AddType("Graph", graphType);
        //DGraph
        var dgraphType = new Type(new Token { type = TokenType.IDENTIFIER, lexeme = "DGraph" }, null);
        dgraphType.SetSuperClass(new Token { type = TokenType.IDENTIFIER, lexeme = "Graph" });
        AddType("DGraph", dgraphType);
        //UGraph
        var ugraphType = new Type(new Token { type = TokenType.IDENTIFIER, lexeme = "UGraph" }, null);
        ugraphType.SetSuperClass(new Token { type = TokenType.IDENTIFIER, lexeme = "Graph" });
        AddType("UGraph", ugraphType);
    }

    public void AddType(string name, Type type)
    {
        if (globalScope.TryAdd(name, type)) return;

        globalScope[name] = type;
    }

    public bool IsTypeDeclared(string name)
    {
        return globalScope
            .Where(t => t.Value.IsDummyType == false)
            .Any(t => t.Key == name);
    }

    public Type GetType(string name)
    {
        if (!globalScope.TryGetValue(name, out var type)) throw new Exception($"Type {name} not declared");
        return type;
    }
}