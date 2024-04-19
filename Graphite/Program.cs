//args[0] name of the input file
//args[1] name of the output file

using Graphite.Checkers;
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
//            TestClass obj = new TestClass(1, true);
//
//            obj.test(1, true, "Hello World", 'c');
//
//            test[123].test(1, true, "Hello World", 'c');
//
//            """;

var code = """
           #Func<int, int> test
           test(int a) returns int {
               return a;
           }
           
           #Func<int, Func<int>, int> test2
           test2 (Func<int, int> f, int a) returns int {
               return f(a);
           }
           
           #Func<Func<int, Func<int, int>, int>>
           test3 () returns Func<int, Func<int, int>, int> {
               return test2;
           }
           
           int a = test3()(test, 1);
           """;
           // public class TestClass 
           // {
           //     public int x;
           //     public test() returns int 
           //     {
           //         return 1;
           //     }
           //     
           //     public test2() returns Func<int>
           //     {
           //         return test;
           //     }
           // }
           //
           // public class TestClass2
           // {
           //     public TestClass a = new TestClass();
           // }
           //
           // TestClass obj = new TestClass();
           //
           // obj.test();
           //
           // obj.test2()();
           //
           // TestClass2 obj2 = new TestClass2();
           //
           // obj2.a.test2()();
           //
           // public class TestClass3 extends TestClass
           // {
           //     public test3() returns int
           //     {
           //         return test();
           //     }
           // }
           //
           // TestClass3 obj3 = new TestClass3();
           //
           // #obj3.test3();
           // """;

var lexer = new Lexer();
var parser = new Parser();
var checker = new ScopeChecker();

var tokens = lexer.ScanCode(code);
var statements = parser.Parse(tokens);
checker.Check(statements);

Console.ReadKey();

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