namespace Graphite.Lexer;

public class InvalidTokenException(string message, int line) : GraphiteLanguageException(message, line)
{

}