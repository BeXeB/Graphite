using System.Runtime.InteropServices.JavaScript;

namespace Graphite;

public class Lexer
{
    private static Dictionary<string, TokenType> _keywords = new()
    {
        {"mod", TokenType.MOD},
        {"if", TokenType.IF},
        {"else", TokenType.ELSE},
        {"while", TokenType.WHILE},
        {"break", TokenType.BREAK},
        {"continue", TokenType.CONTINUE},
        {"return", TokenType.RETURN},
        {"class", TokenType.CLASS},
        {"public", TokenType.PUBLIC},
        {"private", TokenType.PRIVATE},
        {"str", TokenType.STR},
        {"char", TokenType.CHAR},
        {"int", TokenType.INT},
        {"dec", TokenType.DEC},
        {"bool", TokenType.BOOL},
        {"void", TokenType.VOID},
        {"true", TokenType.TRUE},
        {"false", TokenType.FALSE},
        {"null", TokenType.NULL},
        {"new", TokenType.NEW},
        {"this", TokenType.THIS},
        {"super", TokenType.SUPER},
        {"returns", TokenType.RETURNS},
        //{"struct", TokenType.STRUCT},
        {"extends",TokenType.EXTENDS},
        {"Func", TokenType.FUNC_TYPE},
        {"V", TokenType.V},
        {"and", TokenType.AND},
        {"or", TokenType.OR}
    };
    
    private int line = 1;
    private int currentIndex = 0;
    private int tokenStartIndex = 0;
    private string code;
    private List<Token> tokens;
    
    public List<Token> ScanCode(string rawCode)
    {
        tokens = new ();
        code = rawCode;
        while (currentIndex < code.Length)
        {
            tokenStartIndex = currentIndex;
            ScanToken();
        }
        AddToken(TokenType.EOF);
        currentIndex = 0;
        return tokens;
    }
    
    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '#':
                //ignore the rest of the line
                while (Peek() != '\n' && currentIndex < code.Length)
                {
                    Advance();
                }
                break;
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;
            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case '[':
                AddToken(TokenType.LEFT_BRACKET);
                break;
            case ']':
                AddToken(TokenType.RIGHT_BRACKET);
                break;
            case '=':
                if (Match('='))
                {
                    AddToken(TokenType.EQUAL_EQUAL);
                }
                else if (Match('>'))
                {
                    AddToken(TokenType.ARROW);
                }
                else if (Match('/'))
                {
                    if (Match('='))
                    {
                        AddToken(TokenType.SLASHED_EQUAL);
                    }
                    else
                    {
                        throw new InvalidTokenException(line + " : Expected: =");
                    
                    }
                }
                else
                {
                    AddToken(TokenType.EQUAL);
                }
                break;
            case '+':
                if (Match('+'))
                {
                    AddToken(TokenType.PLUS_PLUS);
                }
                else
                {
                    AddToken(TokenType.PLUS);
                }
                break;
            case '-':
                if (Match('-'))
                {
                    AddToken(TokenType.MINUS_MINUS);
                }
                else
                {
                    AddToken(TokenType.MINUS);
                }
                break;
            case '*':
                AddToken(TokenType.STAR);
                break;
            case '/':
                AddToken(TokenType.SLASH);
                break;
            case '!':
                if (Match('='))
                {
                    AddToken(TokenType.BANG_EQUAL);
                }
                else
                {
                    AddToken(TokenType.BANG);
                }
                break;
            case ';':
                AddToken(TokenType.SEMICOLON);
                break;
            case ',':
                AddToken(TokenType.COMMA);
                break;
            case '.':
                AddToken(TokenType.DOT);
                break;
            case '<':
                if (Match('='))
                {
                    if (Match('>'))
                    {
                        AddToken(TokenType.DOUBLE_ARROW);
                    }
                    else
                    {
                        AddToken(TokenType.LESS_EQUAL);
                    }
                }
                else if (Match('<'))
                {
                    AddToken(TokenType.LEFT_LEFT);
                }
                else
                {
                    AddToken(TokenType.LESS);
                }
                break;
            case '>':
                if (Match('='))
                {
                    AddToken(TokenType.GREATER_EQUAL);
                }
                else
                {
                    AddToken(TokenType.GREATER);
                }
                break;
            case '\'':
                //char literal
                //take next character and match next '
                if (char.IsLetterOrDigit(Peek()))
                {
                    Advance();
                    if (Match('\''))
                    {
                        AddToken(TokenType.CHAR_LITERAL, code.Substring(tokenStartIndex + 1, currentIndex - tokenStartIndex - 2));
                    }
                    else
                    {
                        throw new InvalidTokenException(line + " : Expected: '");
                    }
                }
                else
                {
                    throw new InvalidTokenException(line + " : Expected a character");
                }

                break;
            case '"':
                //string literal
                //check next character until we find another double quote
                while (Peek() != '"' && Peek() != '\n' && currentIndex < code.Length)
                {
                    Advance();
                }
                if (Peek() == '\n')
                {
                    throw new InvalidTokenException(line + " : Expected: \"");
                }
                Advance();
                AddToken(TokenType.STRING_LITERAL, code.Substring(tokenStartIndex + 1, currentIndex - tokenStartIndex - 2));
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            default:
                if (char.IsDigit(c))
                {
                    Number();
                }
                else if (char.IsLetter(c))
                {
                    Identifier();
                }
                else
                {
                    throw new InvalidTokenException(line + " : Unexpected character: " + c);
                }
                break;
        }
    }

    private void Number()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }
        
        if (Peek() == '.' && char.IsDigit(Peek(1)))
        {
            Advance();
            while (char.IsDigit(Peek()))
            {
                Advance();
            }
            
            AddToken(TokenType.DECIMAL_LITERAL, decimal.Parse(code.Substring(tokenStartIndex, currentIndex - tokenStartIndex)));
            return;
        }
        
        AddToken(TokenType.INT_LITERAL, int.Parse(code.Substring(tokenStartIndex, currentIndex - tokenStartIndex)));
    }
    
    private void Identifier()
    {
        while (char.IsLetterOrDigit(Peek()))
        {
            Advance();
        }
        
        var text = code.Substring(tokenStartIndex, currentIndex - tokenStartIndex);
        if (_keywords.ContainsKey(text))
        {
            AddToken(_keywords[text]);
        }
        else
        {
            AddToken(TokenType.IDENTIFIER);
        }
    }

    private char Advance()
    {
        return code[currentIndex++];
    }
    
    private char Peek(int depth = 0)
    {
        if (currentIndex + depth >= code.Length) return '\0';
        return code[currentIndex + depth];
    }
    
    private bool Match(char expected)
    {
        if (currentIndex >= code.Length) return false;
        if (code[currentIndex] != expected) return false;
        currentIndex++;
        return true;
    }
    
    private void AddToken(TokenType type, object literal = null)
    {
        var lexeme = code.Substring(tokenStartIndex, currentIndex - tokenStartIndex);
        var token = new Token
        {
            lexeme = lexeme,
            literal = literal,
            type = type,
            line = line,
            startIndex = tokenStartIndex
        };
        //do stuff based with token if we need to
        tokens.Add(token);
    }
}