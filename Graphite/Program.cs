//args[0] name of the input file
//args[1] name of the output file

using Graphite;
using Graphite.Lexer;
using Graphite.Parser;

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
var parser = new Parser();

var tokens = lexer.ScanCode(code);

Console.ReadKey();