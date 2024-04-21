//args[0] name of the input file
//args[1] name of the output file

using Graphite.Lexer;
using Graphite.Parser;
using Domain;
using Graphite;
using QuikGraphVisualizer;

// var code = """
//            public class TestClass {
//                public int x;
//                private bool b;
//                
//                public TestClass(int x, bool b) returns void {
//                    this.x = x;
//                    this.b = b;
//                }
//                
//                public test(int a, bool b, str s, char c) returns int {
//                    return a;
//                }
//            }
//            
//            dec x = 1.2;
//            bool b = true;
//            int a = 0;
//            str s = "Hello World";
//            char c = 'c';
//            Set<int> set = {1, 2, 3, 4};
//            List<int> list = [1, 2, 3, 4];
//            
//            test(int a, bool b, str s, char c) returns int {
//                return a;
//            }
//            
//            Func<int, int, bool, str, char> f = test;
//            
//            G{
//                [("tag1" and "tag2") or "tag3"] => ["tag4"] d;
//                [("tag1" and !"tag2") or "tag3"] <=> ["tag4"] 2;
//                [!("tag1" and "tag2") or "tag3"] <=> ["tag4"];
//                [("tag1" and "tag2") or "tag3"] =/= ["tag4"];
//                V - ["tag1"];
//                V + {"tag1", "tag2"} 12;
//                ["tag3"] ++ {"tag4"};
//                ["tag4"] -- {"tag3"};
//                "tag1" << null;
//                "tag2" << "tag1";
//                a = 1;
//                while (a < 10) {
//                     V + {"tag1", "tag2"};
//                     a = a + 1;
//                }
//                if (a == 10) {
//                     V + {"tag1", "tag2"};
//                } else {
//                     V - ["tag1"];
//                }
//            };
//            
//            Func<void, int> a = () => {
//                return 1;
//            };
//            
//            test();
//            
//            asd.test().test().test();
//            
//            asd.asd.test();
//            
//            TestClass obj = new TestClass(1, true);
//            
//            obj.test(1, true, "Hello World", 'c');
//            
//            test[123].test(1, true, "Hello World", 'c');
//            
//            b = (3 + 3) * 3 > 10;
//            
//            c = test();
//            """;

var code = """
           test() returns Set<int> {
                return {1, 2, 3, 4};
           }
           test()[1];
           test2() returns Set<Func<Set<int>>> {
                return {test};
           }
           test2()[0]()[2];
           # allow this to happen, need to change grammar
           
           test()();
           asd.test[1].test()()();
           asd[1]()();
           asd.test().test().test();
           asd.test[1]();
           test();
           asd.asd.test();
           """;

var lexer = new Lexer();
var parser = new Parser();
var transpiler = new Transpiler();

var tokens = lexer.ScanCode(code);
var statements = parser.Parse(tokens);
var cscode = transpiler.Transpile(statements);

// Console.ReadKey();

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
ugraph.AddVertex(["asd"], 5);
ugraph.AddVertices([["f", "g"], ["h", "i"]]);
ugraph.Retag("a", "e");
ugraph.AddTags(v => v.Contains("e") || v.Contains("h"), ["j", "i"]);
ugraph.RemoveTags(v => v.Contains("j") || v.Contains("e"), ["j", "b"]);
ugraph.Connect(v => v.Contains("e"), v => v.Contains("c"), 123);
ugraph.Connect(v => v.Contains("f"), v => v.Contains("h"), 123.123);
ugraph.PrintGraphInfo();
ugraph.RemoveVertex(v => v.Contains("asd"));

ugraph.PrintGraphInfo();
dgraph.PrintGraphInfo();


// Uncomment to visualize the graph in the browser
//GraphVisualizer<bool>.VisualizeInBrowser(dgraph);