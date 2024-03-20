//args[0] name of the input file
//args[1] name of the output file

using Graphite;
using Graphite.Lexer;
using Graphite.Parser;

// var code = """
//            111.111
//            return
//            (1 + 2) * 3;
//            str string = "Hello World";
//            int x = 5;
//            char c = 'c';
//            and or == != <= >=
//            [] {} () ; , . + - * / ! = < >
//            if else while break continue return class public private str char int dec bool void true false null new this super returns
//            Func<int,bool>
//            """;
var code = """
           G{
               [("tag1" and "tag2") or "tag3"] => ["tag4"] 1.2;
               [("tag1" and !"tag2") or "tag3"] <=> ["tag4"] 2;
               [!("tag1" and "tag2") or "tag3"] <=> ["tag4"];
               [("tag1" and "tag2") or "tag3"] =/= ["tag4"];
               V - ["tag1"];
               V + {"tag1", "tag2"} 12;
               ["tag3"] ++ {"tag4"};
               ["tag4"] -- {"tag3"};
               "tag1" << null;
               "tag2" << "tag1";
               a = 1;
               while (a < 10) {
                    V + {"tag1", "tag2"};
                    a = a + 1;
               }
           };
           """;
var lexer = new Lexer();
var parser = new Parser();

var tokens = lexer.ScanCode(code);
var statements = parser.Parse(tokens);

Console.ReadKey();