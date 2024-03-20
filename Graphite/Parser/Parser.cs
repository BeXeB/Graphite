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
            statements.Add(GraphStmt());
        }
        return statements;
    }

    #region GraphStmt
    
    private Statement.GraphStatement GraphStmt()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier.");
        if (!Match(TokenType.LEFT_BRACE)) throw new ParseException("Expect '{' after identifier.");
        var expressions = new List<GraphExpression>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            expressions.Add(GraphExpr());
        }
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.GraphStatement(identifier, expressions);
    }
    
    private GraphExpression GraphExpr()
    {
        var token = Peek();
        switch (token.type)
        {
            case TokenType.V:
                return GraphVertexExpr();
            case TokenType.LEFT_BRACKET:
                return GraphPredicateExpr();
            case TokenType.STRING_LITERAL:
                return GraphReTagExpr();
            case TokenType.WHILE:
                return GraphWhileStmt();
            case TokenType.IF:
                return GraphIfStmt();
            default:
                return GraphExpressionStmt();
        }
    }
    
    private GraphExpression GraphVertexExpr()
    {
        Consume(TokenType.V, "Expect 'V' at the beginning of the expression.");
        var token = Peek();
        switch (token.type)
        {
            case TokenType.PLUS:
                return GraphAddVertexExpr();
            case TokenType.MINUS:
                return GraphRemoveVertexExpr();
            default:
                throw new ParseException("Expect '+' or '-' after 'V'.");
        }
    }
    
    private GraphExpression.GraphAddVertexExpression GraphAddVertexExpr()
    {
        GraphExpression.GraphAddVertexExpression expression;
        Consume(TokenType.PLUS, "Expect '+' after 'V'.");
        var tags = Set();
        expression = Peek().type == TokenType.INT_LITERAL
            ? new GraphExpression.GraphAddVertexExpression(tags, Advance()) 
            : new GraphExpression.GraphAddVertexExpression(tags, new Token { type = TokenType.INT_LITERAL, lexeme = "", literal = 1 });
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
        return expression;
    }
    
    private GraphExpression.GraphRemoveVertexExpression GraphRemoveVertexExpr()
    {
        GraphExpression.GraphRemoveVertexExpression expression;
        Consume(TokenType.MINUS, "Expect '-' after 'V'.");
        var predicate = Predicate();
        expression = new GraphExpression.GraphRemoveVertexExpression(predicate);
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
        return expression;
    }

    private GraphExpression GraphPredicateExpr()
    {
        var predicate = Predicate();
        var token = Peek();
        switch (token.type)
        {
            case TokenType.ARROW:
            case TokenType.DOUBLE_ARROW:
            case TokenType.SLASHED_EQUAL:
                Consume(token.type, "Expect '=>', '<=>' or '=/=' after predicate.");
                var right = Predicate();
                var peek = Peek();
                var weight = peek.type is TokenType.INT_LITERAL or TokenType.DECIMAL_LITERAL
                    ? Advance()
                    : new Token { type = TokenType.INT_LITERAL, lexeme = "", literal = 1.0 };
                Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
                return new GraphExpression.GraphEdgeExpression(predicate, token, right, weight);
            case TokenType.PLUS_PLUS:
            case TokenType.MINUS_MINUS:
                Consume(token.type, "Expect '++' or '--' after predicate.");
                var tags = Set();
                Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
                return new GraphExpression.GraphTagExpression(predicate, token, tags);
            default:
                throw new ParseException("Expect '=>', '<=>', '=/=', '++' or '--' after predicate.");
        }
    }
    
    private GraphExpression Predicate()
    {
        Consume(TokenType.LEFT_BRACKET, "Expect '[' at the beginning of a predicate.");
        var graphPredicate = PredicateOr();
        Consume(TokenType.RIGHT_BRACKET, "Expect ']' at the end of a predicate.");
        return graphPredicate;
    }
    
    private GraphExpression PredicateOr()
    {
        var expression = PredicateAnd();
        while (Match(TokenType.OR))
        {
            var @operator = Previous();
            var right = PredicateAnd();
            expression = new GraphExpression.PredicateOrExpression(expression, @operator, right);
        }
        return expression;
    }
    
    private GraphExpression PredicateAnd()
    {
        var expression = PredicateUnary();
        while (Match(TokenType.AND))
        {
            var @operator = Previous();
            var right = PredicateUnary();
            expression = new GraphExpression.PredicateAndExpression(expression, @operator, right);
        }
        return expression;
    }
    
    private GraphExpression PredicateUnary()
    {
        if (Match(TokenType.BANG))
        {
            var @operator = Previous();
            var right = PredicateUnary();
            return new GraphExpression.PredicateUnaryExpression(@operator, right);
        }
        return PredicateLiteral();
    }
    
    private GraphExpression PredicateLiteral()
    {
        var token = Peek();
        if (Match(TokenType.STRING_LITERAL))
        {
            return new GraphExpression.PredicateLiteralExpression(token);
        }
        if (Match(TokenType.LEFT_PAREN))
        {
            var expression = PredicateOr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GraphExpression.PredicateGroupingExpression(expression);
        }
        throw new ParseException("Expect predicate.");
    }
    
    private GraphExpression.GraphReTagExpression GraphReTagExpr()
    {
        var tagToChange = Consume(TokenType.STRING_LITERAL, "Expect string literal.");
        Consume(TokenType.LEFT_LEFT, "Expect '<<' after string literal.");
        var token = Peek();
        switch (token.type)
        {
            case TokenType.STRING_LITERAL:
            case TokenType.NULL:
                Advance();
                Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
                return new GraphExpression.GraphReTagExpression(tagToChange, token);
            default:
                throw new ParseException("Expect string literal or 'null' after '<<'.");
        }
    }
    
    private GraphExpression.GraphWhileStmt GraphWhileStmt()
    {
        Consume(TokenType.WHILE, "Expect 'while' at the beginning of the statement.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expr();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        var body = GraphBlockStmt();
        return new GraphExpression.GraphWhileStmt(condition, body);
    }
    
    private GraphExpression.GraphIfStmt GraphIfStmt()
    {
        Consume(TokenType.IF, "Expect 'if' at the beginning of the statement.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expr();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        var thenBranch = GraphBlockStmt();
        if (Match(TokenType.ELSE))
        {
            var elseBranch = GraphBlockStmt();
            return new GraphExpression.GraphIfStmt(condition, thenBranch, elseBranch);
        }
        return new GraphExpression.GraphIfStmt(condition, thenBranch, null);
    }
    
    private GraphExpression.GraphBlockStmt GraphBlockStmt()
    {
        Consume(TokenType.LEFT_BRACE, "Expect '{' at the beginning of the block.");
        var statements = new List<GraphExpression>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            statements.Add(GraphExpr());
        }
        return new GraphExpression.GraphBlockStmt(statements);
    }
    
    private GraphExpression.GraphExpressionStmt GraphExpressionStmt()
    {
        var statement = ExprStmt();
        return new GraphExpression.GraphExpressionStmt(statement);
    }
    
    #endregion
    
    private Statement ExprStmt()
    {
        while(!Match(TokenType.SEMICOLON))
        {
            Advance();
        }
        return null;
    }

    private Expression Expr()
    {
        while(Peek().type != TokenType.RIGHT_PAREN)
        {
            Advance();
        }
        return null;
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
    
    private Expression Set()
    {
        while (!Match(TokenType.RIGHT_BRACE))
        {
            Advance();
        }
        return null;
    }
    
    private Token Previous()
    {
        return tokens[current - 1];
    }
    
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new ParseException(message);
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
    
    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
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