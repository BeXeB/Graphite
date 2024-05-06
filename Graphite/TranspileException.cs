using Graphite.Lexer;

namespace Graphite;

public class TranspileException(string message, Token token) : GraphiteLanguageException(message, token)
{
    
}