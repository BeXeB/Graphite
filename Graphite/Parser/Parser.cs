using Graphite.Lexer;
using static Graphite.Statement;

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

    private Statement.ClassDeclarationStatement ClassDeclarationStatement()
    {
        Token accessModifier;
        Token classIdentifier;
        Token? extends = null;
        Token? extendsIdentifier = null;
        List<VariableDeclarationStatement> variableDeclarationStatements = new List<VariableDeclarationStatement>();
        List<FunctionDeclarationStatement> functionDeclarationStatements = new List<FunctionDeclarationStatement>();

        accessModifier = Peek();

        if (accessModifier.type != TokenType.PUBLIC && accessModifier.type != TokenType.PRIVATE)
        {
            throw new InvalidTokenException("Invalid or missing access modifier");
        }

        Advance();

        classIdentifier = Consume(TokenType.IDENTIFIER, "Invalid class identifier");

        if (Peek().type == TokenType.EXTENDS)
        {
            extends = Consume(TokenType.EXTENDS, "Unexpected internal error in parser");
            extendsIdentifier = Consume(TokenType.IDENTIFIER, "Extending invalid identifier");
        }

        Consume(TokenType.LEFT_BRACE, "Expecting '{' after identifier at class decleration");

        while (Peek().type != TokenType.RIGHT_BRACE)
        {
            accessModifier = Peek();

            if (accessModifier.type != TokenType.PUBLIC && accessModifier.type != TokenType.PRIVATE)
            {
                throw new InvalidTokenException("Invalid or missing access modifier");
            }

            Advance();

            // TODO: discuss if we really want óur current syntax or the variable/function syntax like c#/java/c/..
            switch(Peek(1).type)
            {
                case TokenType.LEFT_PAREN: // Meaning its a function decleration
                    functionDeclarationStatements.Add(FunctionDeclarationStatement());
                    break;
                case TokenType.IDENTIFIER: // Meaning its a variable decleration
                    variableDeclarationStatements.Add(VariableDeclarationStatement());
                    break;
                default:
                    throw new InvalidTokenException("Unexpected token inside class decleration. Expected class- or function decleration");
            }   
        }

        return new ClassDeclarationStatement(
            accessModifier,
            classIdentifier,
            extends,
            extendsIdentifier,
            variableDeclarationStatements,
            functionDeclarationStatements
            );
    }

    private Statement.VariableDeclarationStatement VariableDeclarationStatement()
    {
        Token type = Peek();
        //Token? functionTypeReturnType;
        //List<Token> functionTypeArguments;
        Token identifier;
        Expression? initializingExpression = null;

        switch (type.type)
        {
            case TokenType.STR:
            case TokenType.CHAR:
            case TokenType.INT:
            case TokenType.DEC:
            case TokenType.BOOL:
            case TokenType.IDENTIFIER:
                Advance();
                break;
            case TokenType.FUNC_TYPE:
                Consume(TokenType.LESS, "expected '<' after Func type to declare return- and argument types");
                //functionTypeReturnType = 
                //TODO we need to figure out how to solve this problem with types that can be of tokentype.type, but als of type
                // Func, that has to contain other types.
                throw new NotImplementedException("We need to figure out how to solve this problem");
            default:
                throw new InvalidTokenException("Invalid or missing type for variable declaration");
        }

        identifier = Consume(TokenType.IDENTIFIER, "missing identifier after variable type declaration");
        
        if (Peek().type == TokenType.EQUAL)
        {
            Advance();
            initializingExpression = Expression();
        }

        Consume(TokenType.SEMICOLON, "missing semicolon");

        return new VariableDeclarationStatement(type, identifier, initializingExpression );
    }

    private Statement.FunctionDeclarationStatement FunctionDeclarationStatement()
    {
        Token identifier = Consume(TokenType.IDENTIFIER, "expecting function identifier");
        throw new NotImplementedException("Malthe in progress here lel"); //TODO finnish
    }

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
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        var body = GraphBlockStmt();
        return new GraphExpression.GraphWhileStmt(condition, body);
    }
    
    private GraphExpression.GraphIfStmt GraphIfStmt()
    {
        Consume(TokenType.IF, "Expect 'if' at the beginning of the statement.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
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
    
    private Statement ExprStmt()
    {
        while(!Match(TokenType.SEMICOLON))
        {
            Advance();
        }
        return null;
    }

    private Expression Expression()
    {
        while(Peek().type != TokenType.RIGHT_PAREN)
        {
            Advance();
        }
        return null;
    }

    private Expression Additive()
    {
        Consume(TokenType.STRING_LITERAL, "Expect string literal.");
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
    
    private Token Consume(TokenType type, string errorMessage)
    {
        if (Check(type)) return Advance();
        throw new ParseException(errorMessage);
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
    
    private Token Peek(int steps = 0)
    {
        return tokens[current + steps];
    }
}