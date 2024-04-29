//args[0] name of the input file
//args[1] name of the output file

using System.Reflection;
using Graphite.Lexer;
using Graphite.Parser;
using Graphite;
using Graphite.Checkers;

internal class Program
{
    public static void Main(string[] args)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var path = basePath.Remove(basePath.IndexOf("Graphite", StringComparison.Ordinal)) +
                   @"Graphite\Graphite\code.txt";
        var code = File.ReadAllText(path);

        var lexer = new Lexer();
        var parser = new Parser();
        var checker = new SemanticChecker();
        var transpiler = new Transpiler();

        var tokens = lexer.ScanCode(code);
        var statements = parser.Parse(tokens);
        checker.Check(statements);
        var cscode = transpiler.Transpile(statements);

        var outputPath = basePath.Remove(basePath.IndexOf("Graphite", StringComparison.Ordinal)) +
                         @"Graphite\Graphite\output.cs";
        File.WriteAllText(outputPath, cscode);
    }
}

//GraphVisualizer<bool>.VisualizeInBrowser(dgraph);