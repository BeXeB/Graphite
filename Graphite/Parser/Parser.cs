using Graphite.Lexer;
using static Graphite.Statement;

namespace Graphite.Parser;

public class Parser
{
    private int current;
    private List<Token> tokens = new();
    
    public List<Statement> Parse(List<Token> tokens)
    {
        this.tokens = tokens;
        var statements = new List<Statement>();
        current = 0;
        while (!IsAtEnd())
        {
            try
            {
                statements.Add(Declaration());
            }
            catch (ParseException e)
            {
                Console.WriteLine(e);
                Synchronize();
            }
        }
        return statements;
    }

    #region Statements
    
    private Statement Declaration()
    {
        return null;
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

        Consume(TokenType.LEFT_BRACE, "Expecting '{' after identifier at class declaration");

        while (Peek().type != TokenType.RIGHT_BRACE)
        {
            accessModifier = Peek();

            if (accessModifier.type != TokenType.PUBLIC && accessModifier.type != TokenType.PRIVATE)
            {
                throw new InvalidTokenException("Invalid or missing access modifier");
            }

            Advance();

            // TODO: discuss if we really want óur current syntax or the variable/function syntax like c#/java/c/..
            switch (Peek(1).type)
            {
                case TokenType.LEFT_PAREN: // Meaning its a function declaration
                    functionDeclarationStatements.Add(FunctionDeclarationStatement());
                    break;
                case TokenType.IDENTIFIER: // Meaning its a variable declaration
                    variableDeclarationStatements.Add(VariableDeclarationStatement());
                    break;
                default:
                    throw new InvalidTokenException("Unexpected token inside class declaration. Expected class- or function declaration");
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
        OtherNonTerminals.Type type;
        Token identifier;
        Expression? initializingExpression = null;

        type = Type();

        identifier = Consume(TokenType.IDENTIFIER, "missing identifier after variable type declaration");

        if (Peek().type == TokenType.EQUAL)
        {
            Advance();
            initializingExpression = Expression();
        }

        Consume(TokenType.SEMICOLON, "missing semicolon");

        return new VariableDeclarationStatement(type, identifier, initializingExpression);
    }

    private Statement.FunctionDeclarationStatement FunctionDeclarationStatement()
    {
        Token identifier;
        OtherNonTerminals.Parameters parameters;
        BlockStatement blockStatement;

        identifier = Consume(TokenType.IDENTIFIER, "expecting function identifier");

        parameters = Parameters();
        blockStatement = BlockStatement();

        return new FunctionDeclarationStatement(identifier, parameters, blockStatement);
    }

    private OtherNonTerminals.Parameters Parameters()
    {
        var parameters = new List<(OtherNonTerminals.Type, Token)>();

        Consume(TokenType.LEFT_PAREN, "missing left parentheses before parameters");

        bool firstParameter = true;

        while (Peek().type != TokenType.RIGHT_PAREN)
        {
            if (!firstParameter)
            {
                Consume(TokenType.COMMA, "expecting comma separation between parameters");
            }

            OtherNonTerminals.Type parameterType = Type();
            Token parameterIdentifier = Consume(TokenType.IDENTIFIER, "expecting identifier for parameter");

            parameters.Add(new(parameterType, parameterIdentifier));

            firstParameter = false;
        }

        return new OtherNonTerminals.Parameters(parameters);
    }

    private OtherNonTerminals.Type Type()
    {
        Token type = Peek();
        List<OtherNonTerminals.Type> typeArguments = new List<OtherNonTerminals.Type>();

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

                bool firstArgument = true;

                while (Peek().type != TokenType.GREATER)
                {
                    if (!firstArgument)
                    {
                        Consume(TokenType.COMMA, "expecting comma separation between type arguments");
                    }

                    OtherNonTerminals.Type argumentType = Type();

                    typeArguments.Add(argumentType);

                    firstArgument = false;
                }

                Consume(TokenType.GREATER, "expecting type arguments to end with an >");
                break;
            default:
                throw new InvalidTokenException("Invalid or missing type");
        }

        return new OtherNonTerminals.Type(type, typeArguments);
    }

    private Statement Statement()
    {
        var token = Peek();
        switch (token.type)
        {
            case TokenType.IF:
                return IfStatement();
            case TokenType.WHILE:
                return WhileStatement();
            case TokenType.RETURN:
                return ReturnStatement();
            case TokenType.BREAK:
                return BreakStatement();
            case TokenType.CONTINUE:
                return ContinueStatement();
            case TokenType.IDENTIFIER:
                return GraphStatement();
            case TokenType.LEFT_BRACE:
                return BlockStatement();
            default:
                return ExpressionStatement();
        }
    }
    
    private Statement.IfStatement IfStatement()
    {
        return null;
    }
    
    private Statement.WhileStatement WhileStatement()
    {
        return null;
    }
    
    private Statement.ReturnStatement ReturnStatement()
    {
        Consume(TokenType.RETURN, "Expect 'return' at the beginning of the statement.");
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.ReturnStatement(value);
    }
    
    private Statement.BreakStatement BreakStatement()
    {
        Consume(TokenType.BREAK, "Expect 'break' at the beginning of the statement.");
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.BreakStatement();
    }
    
    private Statement.ContinueStatement ContinueStatement()
    {
        Consume(TokenType.CONTINUE, "Expect 'continue' at the beginning of the statement.");
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.ContinueStatement();
    }

    private Statement.BlockStatement BlockStatement()
    {
        var statements = new List<Statement>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            try
            {
                statements.Add(Declaration());
            }
            catch (ParseException e)
            {
                Console.WriteLine(e);
                Synchronize();
            }
        }
        return new Statement.BlockStatement(statements);
    }
    
    private Statement ExpressionStatement()
    {
        var expression = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement.");
        return new Statement.ExpressionStatement(expression);
    }
    
    #endregion

    #region GraphStatement

    private Statement.GraphStatement GraphStatement()
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
                return GraphWhileStatement();
            case TokenType.IF:
                return GraphIfStatement();
            default:
                return GraphExpressionStatement();
        }
    }

    private GraphExpression VertexOperation()
    {
        Consume(TokenType.V, "Expect 'V' at the beginning of the expression.");
        var token = Peek();
        switch (token.type)
        {
            case TokenType.PLUS:
                return GraphAddVertexExpression();
            case TokenType.MINUS:
                return GraphRemoveVertexExpression();
            default:
                throw new ParseException("Expect '+' or '-' after 'V'.");
        }
    }

    private GraphExpression.GraphAddVertexExpression GraphAddVertexExpression()
    {
        Consume(TokenType.PLUS, "Expect '+' after 'V'.");
        var tags = Set();
        var expression = Peek().type == TokenType.INT_LITERAL
            ? new GraphExpression.GraphAddVertexExpression(tags, Advance()) 
            : new GraphExpression.GraphAddVertexExpression(tags, new Token { type = TokenType.INT_LITERAL, lexeme = "", literal = 1 });
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression.");
        return expression;
    }

    private GraphExpression.GraphRemoveVertexExpression GraphRemoveVertexExpression()
    {
        Consume(TokenType.MINUS, "Expect '-' after 'V'.");
        var predicate = Predicate();
        var expression = new GraphExpression.GraphRemoveVertexExpression(predicate);
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

    private GraphExpression.GraphWhileStatement GraphWhileStatement()
    {
        Consume(TokenType.WHILE, "Expect 'while' at the beginning of the statement.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        var body = GraphBlockStatement();
        return new GraphExpression.GraphWhileStatement(condition, body);
    }

    private GraphExpression.GraphIfStatement GraphIfStatement()
    {
        Consume(TokenType.IF, "Expect 'if' at the beginning of the statement.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
        var thenBranch = GraphBlockStatement();
        if (Match(TokenType.ELSE))
        {
            var elseBranch = GraphBlockStatement();
            return new GraphExpression.GraphIfStatement(condition, thenBranch, elseBranch);
        }
        return new GraphExpression.GraphIfStatement(condition, thenBranch, null);
    }

    private GraphExpression.GraphBlockStatement GraphBlockStatement()
    {
        Consume(TokenType.LEFT_BRACE, "Expect '{' at the beginning of the block.");
        var statements = new List<GraphExpression>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            statements.Add(GraphOperation());
        }
        return new GraphExpression.GraphBlockStatement(statements);
    }

    private GraphExpression.GraphExpressionStatement GraphExpressionStatement()
    {
        var statement = ExpressionStatement();
        return new GraphExpression.GraphExpressionStatement(statement);
    }

    #endregion

    #region Expressions
    
    private Expression Expression()
    {
        return Assignment();
    }
    
    private Expression Assignment()
    {
        var expression = NonAssignment();
        if (!Match(TokenType.EQUAL)) return expression;
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
        var body = BlockStatement();
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
        while (true)
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
        if(Match(TokenType.MINUS) || Match(TokenType.BANG))
        {
            var @operator = Previous();
            var right = Unary();
            var expression = new Expression.UnaryExpression(@operator, right);
            return expression;
        }
        return Call();
    }

    private Expression Call()
    {
        var expression = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                var arguments = Arguments();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
                expression = new Expression.CallExpression(expression, arguments);
            }
            if (Match(TokenType.DOT))
            {
                var field = Call();
                expression = new Expression.GetFieldExpression(expression, field);
            }
            else
            {
                break;
            }
        }
        
        return expression;
    }

    private Expression Primary()
    {
        return null;
    }

    private Expression Set()
    {
        Consume(TokenType.LEFT_BRACE, "Expect '{' at the beginning of the set.");
        var elements = Arguments();
        Consume(TokenType.RIGHT_BRACE, "Expect '}' at the end of the set.");
        return new Expression.SetExpression(elements);
    }

    private Expression List()
    {
        Consume(TokenType.LEFT_BRACKET, "Expect '[' at the beginning of the list.");
        var elements = Arguments();
        Consume(TokenType.RIGHT_BRACKET, "Expect ']' at the end of the list.");
        return new Expression.ListExpression(elements);
    }

    private Expression ElementAccess()
    {
        var token = Consume(TokenType.IDENTIFIER, "Expect identifier.");
        Expression expression = new Expression.VariableExpression(token);
        while (Match(TokenType.LEFT_BRACKET))
        {
            var index = NonAssignment();
            Consume(TokenType.RIGHT_BRACKET, "Expect ']' after index.");
            expression = new Expression.ElementAccessExpression(expression, index);
        }
        return expression;
    }

    
    #endregion

    #region Helpers
    
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

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON) return;
            switch (Peek().type)
            {
                case TokenType.INT:
                case TokenType.DEC:
                case TokenType.BOOL:
                case TokenType.FUNC_TYPE:
                case TokenType.STR:
                case TokenType.CHAR:
                case TokenType.BREAK:
                case TokenType.CONTINUE:
                case TokenType.PRIVATE:
                case TokenType.PUBLIC:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.RETURN:
                case TokenType.IDENTIFIER:
                    return;
            }
            Advance();
        }
    }

    #endregion
}