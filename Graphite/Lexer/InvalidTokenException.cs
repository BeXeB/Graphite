namespace Graphite.Lexer;

public class InvalidTokenException(string message, Token token) : GraphiteLanguageException(message, token)
{

}