using Graphite.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphite.Parser;

namespace Graphite.Checkers
{
    internal class CheckException(string message, ILanguageConstruct construct) : GraphiteLanguageException(message, construct.Line) { }
}
