using Graphite.Lexer;

namespace Graphite.Parser;

public class ParseException(string message, Token token) : GraphiteLanguageException(message, token.line) {}