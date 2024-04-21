using Graphite.Lexer;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers;

internal class TypeTable
{
    private Dictionary<string, Type> globalScope = new Dictionary<string, Type>();

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