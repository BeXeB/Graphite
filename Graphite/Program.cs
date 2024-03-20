//args[0] name of the input file
//args[1] name of the output file

using Domain;
using Graphite;

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

var graph = new DGraph();
graph.AddVertex(["a","b"]);
graph.AddVertex(["c","d"]);
graph.AddVertices([["f", "g"], ["h","i"]]);
graph.Retag("a","e");
graph.Connect(v => v.Contains("e"), v => v.Contains("c"));