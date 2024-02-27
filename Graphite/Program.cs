//args[0] name of the input file
//args[1] name of the output file

using Graphite;

Console.WriteLine("Hello World!");

var code = """
           111.111
           return
           (1 + 2) * 3; 
           (Vertex v) => {v.x + v.y};
           str string = "Hello World";
           int x = 5;
           char c = 'c';
           || && == != <= >=
           [] {} () ; , . + - * / ! = < >
           if else while break continue return class public private str char int dec bool void true false null new this super returns struct
           """;
var lexer = new Lexer();

var tokens = lexer.ScanCode(code);

Console.WriteLine("Tokens:");