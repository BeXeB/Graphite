using Graphite.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphite
{
    public class GraphiteLanguageException : Exception
    {
        public GraphiteLanguageException(string errorMessage, int line) : base(errorMessage + ". At line: " + line)
        {
        }
    }
}
