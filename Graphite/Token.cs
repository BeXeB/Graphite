namespace Graphite;

public struct Token
{
    public TokenType type;
    public string lexeme;
    public object literal;
    public int line;
    public int startIndex;
}

public enum TokenType
{
    // Single-character tokens
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, LEFT_BRACKET, 
    RIGHT_BRACKET, EQUAL, PLUS, MINUS, STAR, SLASH, BANG, 
    SEMICOLON, COMMA, DOT, LESS, GREATER,
    
    // One or two character tokens
    AND, OR, EQUAL_EQUAL, BANG_EQUAL, LESS_EQUAL, GREATER_EQUAL, ARROW,
    
    // Literals
    IDENTIFIER, STRING_LITERAL, CHAR_LITERAL, INT_LITERAL, 
    DECIMAL_LITERAL,
    
    // Keywords
    MOD, IF, ELSE, WHILE, BREAK, CONTINUE, RETURN, CLASS, PUBLIC, PRIVATE,
    STR, CHAR, INT, DEC, BOOL, VOID, TRUE, FALSE, NULL, NEW, THIS, SUPER,
    RETURNS, STRUCT, 
    
    // Misc
    EOF
}