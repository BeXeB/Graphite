using Graphite.Lexer;

namespace Graphite.Parser;

public class Parser
{
    private int current = 0;
    private List<Token> tokens;
    
    public List<Statement> Parse(List<Token> tokens)
    {
        this.tokens = tokens;
        var statements = new List<Statement>();
        current = 0;
        while (!IsAtEnd())
        {
            //statements.Add(Declaration());
        }
        return statements;
    }
    
    private Expression Or()
    {
        var expression = And();

        while (Match(TokenType.OR))
        {
            var @operator = Previous();
            var right = And();
            expression = new Expression.LogicalExpression(expression, @operator, right);
        }
        
        return expression;
    }
    
    private Expression And()
    {
        return null;
    }
    
    private Token Previous()
    {
        return tokens[current - 1];
    }
    
    private bool Match(TokenType type)
    {
        if (!Check(type)) return false;
        Advance();
        return true;
    }
    
    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }
    
    private void Advance()
    {
        if (!IsAtEnd()) current++;
    }
    
    private bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }
    
    private Token Peek()
    {
        return tokens[current];
    }
}