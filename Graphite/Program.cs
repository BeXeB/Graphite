//args[0] name of the input file
//args[1] name of the output file

using Graphite.Lexer;
using Graphite.Parser;

var code = """
           public class TestClass {
               public int x;
               private bool b;
               
               public TestClass(int x, bool b) returns void {
                   this.x = x;
                   this.b = b;
               }
               
               public test(int a, bool b, str s, char c) returns int {
                   return a;
               }
           }
           
           dec x = 1.2;
           bool b = true;
           int a = 0;
           str s = "Hello World";
           char c = 'c';
           Set<int> set = {1, 2, 3, 4};
           List<int> list = [1, 2, 3, 4];
           
           test(int a, bool b, str s, char c) returns int {
               return a;
           }
           
           Func<int, int, bool, str, char> f = test;
           
           G{
               [("tag1" and "tag2") or "tag3"] => ["tag4"] d;
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
               if (a == 10) {
                    V + {"tag1", "tag2"};
               } else {
                    V - ["tag1"];
               }
           };
           
           Func<void, int> a = () => {
               return 1;
           };
           
           TestClass obj = new TestClass(1, true);
           
           obj.test(1, true, "Hello World", 'c');
           
           test[123].test(1, true, "Hello World", 'c');
           
           """;

var lexer = new Lexer();
var parser = new Parser();

var tokens = lexer.ScanCode(code);
var statements = parser.Parse(tokens);

Console.ReadKey();