using Graphite.Lexer;
using Graphite.Parser;

namespace Graphite;

public class TranspileException(string message, ILanguageConstruct construct) : GraphiteLanguageException(message, construct.Line)
{
    
}