using System.Linq.Expressions;
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
            statements.Add(Declaration());
        }
        return statements;
    }

    #region Statements

    private Statement Declaration()
    {
        return null;
    }
    
    private List<Token>? Parameters()
    {
        return null;
    }
    
    private Statement.BlockStatement BlockStmt()
    {
        var statements = new List<Statement>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            statements.Add(Declaration());
        }
        return new Statement.BlockStatement(statements);
    }
    
    private Statement ExprStmt()
    {
        var expression = Expr();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.ExpressionStatement(expression);
    }
    
    #endregion

    #region GraphStmt
    
    private Statement.GraphStatement GraphStmt()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier.");
        if (!Match(TokenType.LEFT_BRACE)) throw new ParseException("Expect '{' after identifier.");
        var expressions = new List<GraphExpression>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            expressions.Add(GraphOperation());
        }
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.GraphStatement(identifier, expressions);
    }
    
    private GraphExpression GraphOperation()
    {
        var token = Peek();
        switch (token.type)
        {
            case TokenType.V:
                return VertexOperation();
            case TokenType.LEFT_BRACKET:
                return PredOperation();
            case TokenType.STRING_LITERAL:
                return RetagOperation();
            case TokenType.WHILE:
                return GraphWhileStmt();
            case TokenType.IF:
                return GraphIfStmt();
            default:
                return GraphExpressionStmt();
        }
    }
    
    private GraphExpression VertexOperation()
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

    private GraphExpression PredOperation()
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
        if (Match(TokenType.LEFT_PAREN))
        {
            var expression = PredicateOr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GraphExpression.PredicateGroupingExpression(expression);
        }
        var stringExpression = Additive();
        return new GraphExpression.PredicateLiteralExpression(stringExpression);
    }
    
    private GraphExpression.GraphReTagExpression RetagOperation()
    {
        var oldTag = Consume(TokenType.STRING_LITERAL, "Expect string literal.");
        Consume(TokenType.LEFT_LEFT, "Expect '<<' after string literal.");
        var token = Peek();
        if (token.type is not (TokenType.STRING_LITERAL or TokenType.NULL))
            throw new ParseException("Expect string literal or 'null' after '<<'.");
        Advance();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
        return new GraphExpression.GraphReTagExpression(oldTag, token);
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
            statements.Add(GraphOperation());
        }
        return new GraphExpression.GraphBlockStmt(statements);
    }
    
    private GraphExpression.GraphExpressionStmt GraphExpressionStmt()
    {
        var statement = ExprStmt();
        return new GraphExpression.GraphExpressionStmt(statement);
    }
    
    #endregion

    #region Expressions
    
    private Expression Expr()
    {
        return Assignment();
    }
    
    private Expression Assignment()
    {
        var expression = NonAssignment();
        if (!Match(TokenType.EQUAL)) return expression;
        var equals = Previous();
        var value = NonAssignment();
        return expression switch
        {
            Expression.VariableExpression variable => new Expression.AssignmentExpression(variable.name, value),
            Expression.GetFieldExpression get => new Expression.SetFieldExpression(get.obj, get.field, value),
            _ => throw new ParseException("Invalid assignment target.")
        };
    }
    
    private Expression NonAssignment()
    {
        return Peek().type switch
        {
            TokenType.LEFT_PAREN => AnonFunc(),
            TokenType.NEW => Instance(),
            _ => Or()
        };
    }
    
    private Expression AnonFunc()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' at the beginning of the anonymous function.");
        var parameters = Parameters();
        Consume(TokenType.ARROW, "Expect '=>' after parameters.");
        var body = BlockStmt();
        return new Expression.AnonFunctionExpression(parameters, body);
    }

    private Expression Instance()
    {
        Consume(TokenType.NEW, "Expect 'new' at the beginning of the instance creation.");
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier after 'new'.");
        if (!Match(TokenType.LEFT_PAREN)) throw new ParseException("Expect '(' after identifier.");
        var arguments = Arguments();
        return new Expression.InstanceExpression(identifier, arguments);
    }

    private List<Expression> Arguments()
    {
        var arguments = new List<Expression>();
        while (!Match(TokenType.RIGHT_PAREN))
        {
            arguments.Add(NonAssignment());
            if (!Match(TokenType.COMMA)) break;
        }
        return arguments;
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
        var expression = Equality();

        while (Match(TokenType.AND))
        {
            var @operator = Previous();
            var right = Equality();
            expression = new Expression.LogicalExpression(expression, @operator, right);
        }

        return expression;
    }

    private Expression Equality()
    {
        var expression = Comparison();

        while (Match(TokenType.EQUAL_EQUAL) || Match(TokenType.BANG_EQUAL)) 
        {
            var @operator = Previous();
            var right = Comparison();
            expression = new Expression.BinaryExpression(expression, @operator, right);
        }

        return expression;
    }

    private Expression Comparison()
    {
        var expression = Additive();

        while(Match(TokenType.LESS) || Match(TokenType.LESS_EQUAL) || Match(TokenType.GREATER_EQUAL) || Match(TokenType.GREATER))
        {
            var @operator = Previous();
            var right = Additive();
            expression = new Expression.BinaryExpression(expression, @operator, right);
        }
        return expression;
    }

    private Expression Additive()
    {
        var expression = Multiplicative();

        while (Match(TokenType.PLUS) || Match(TokenType.MINUS))
        {
            var @operator = Previous();
            var right = Multiplicative();
            expression = new Expression.BinaryExpression(expression, @operator, right);
        }
        return expression;
    }

    private Expression Multiplicative()
    {
        var expression = Unary();

        while (Match(TokenType.STAR) || Match(TokenType.SLASH) || Match(TokenType.MOD))
        {
            var @operator = Previous();
            var right = Unary();
            expression = new Expression.BinaryExpression(expression, @operator, right);
        }
        return expression;
    }

    private Expression Unary()
    {
        return null;
    }

    private Expression Call()
    {
        return null;
    }

    private Expression Arguments()
    {
        return null;
    }

    private Expression Primary()
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

    private Expression List()
    {
        return null;
    }

    private Expression ElementAccess()
    {
        return null;
    }

    private Expression Elements()
    {
        return null;
    }

    
    #endregion

    #region Helpers
    
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
    
    private Token Peek(int offset = 0)
    {
        return tokens[current+offset];
    }
    
    #endregion
}