//args[0] name of the input file
//args[1] name of the output file

using System.Reflection;
using Graphite.Checkers;
using Graphite.Lexer;
using Graphite.Parser;
using Domain;

var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
var path = basePath.Remove(basePath.IndexOf("Graphite", StringComparison.Ordinal)) + @"Graphite\Graphite\code.txt";
var code = File.ReadAllText(path);

var lexer = new Lexer();
var parser = new Parser();
var checker = new SemanticChecker();

var tokens = lexer.ScanCode(code);
var statements = parser.Parse(tokens);
checker.Check(statements);

var dgraph = new DGraph();
dgraph.AddVertex(["a", "b"]);
dgraph.AddVertex(["c", "d"]);
dgraph.AddVertices([["f", "g"], ["h", "i"]]);
dgraph.Retag("a", "e");
dgraph.AddTags(v => v.Contains("e") || v.Contains("h"), ["j", "i"]);
dgraph.RemoveTags(v => v.Contains("j") || v.Contains("e"), ["j", "b"]);
dgraph.Connect(v => v.Contains("e"), v => v.Contains("c"));
dgraph.Connect(v => v.Contains("f"), v => v.Contains("h"));
dgraph.Connect(v => v.Contains("e"), v => v.Contains("h"));

var ugraph = new UGraph();
ugraph.AddVertex(["a", "b"]);
ugraph.AddVertex(["c", "d"]);
ugraph.AddVertices([["f", "g"], ["h", "i"]]);
ugraph.Retag("a", "e");
ugraph.AddTags(v => v.Contains("e") || v.Contains("h"), ["j", "i"]);
ugraph.RemoveTags(v => v.Contains("j") || v.Contains("e"), ["j", "b"]);
ugraph.Connect(v => v.Contains("e"), v => v.Contains("c"));

var ugraph2 = new UGraph();
ugraph2.AddVertex(["a", "b"]);
ugraph2.AddVertex(["c", "d"]);
ugraph2.AddVertices([["f", "g"], ["h", "i"]]);
ugraph2.Connect(v => v.Contains("a"), v => v.Contains("c"));
ugraph2.Connect(v => v.Contains("f"), v => v.Contains("i"));

ugraph.AddGraph(ugraph2);

// dgraph.PrintGraphInfo();
// ugraph.PrintGraphInfo();

// Uncomment to visualize the graph in the browser
//GraphVisualizer<bool>.VisualizeInBrowser(dgraph);
