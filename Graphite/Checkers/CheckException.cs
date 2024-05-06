using Graphite.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphite.Checkers
{
    internal class CheckException(string message, Token token) : GraphiteLanguageException(message, token) { }
}
