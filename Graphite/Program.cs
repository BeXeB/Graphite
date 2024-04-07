//args[0] name of the input file
//args[1] name of the output file

using Domain;
using Graphite;
using QuikGraphVisualizer;

var code = """
           111.111
           return
           (1 + 2) * 3;
           str string = "Hello World";
           int x = 5;
           char c = 'c';
           and or == != <= >=
           [] {} () ; , . + - * / ! = < >
           if else while break continue return class public private str char int dec bool void true false null new this super returns
           Func<int,bool>
           """;
var lexer = new Lexer();

var tokens = lexer.ScanCode(code);

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

dgraph.PrintGraphInfo();
ugraph.PrintGraphInfo();

/// Uncomment to visualize the graph in the browser
//GraphVisualizer<bool>.VisualizeInBrowser(dgraph);