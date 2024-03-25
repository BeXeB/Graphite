using Graphite.Lexer;
using static Graphite.Parser.OtherNonTerminals;
using static Graphite.Statement;
using System.Linq.Expressions;

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
            switch (Peek(1).type)
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
        OtherNonTerminals.Type type;
        Token identifier;
        Expr? initializingExpression = null;

        type = Type();

        identifier = Consume(TokenType.IDENTIFIER, "missing identifier after variable type declaration");

        if (Peek().type == TokenType.EQUAL)
        {
            Advance();
            initializingExpression = Expr();
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

        Consume(TokenType.LEFT_PAREN, "missing left parantheses before parameters");

        bool firstParameter = true;

        while (Peek().type != TokenType.RIGHT_PAREN)
        {
            if (!firstParameter)
            {
                Consume(TokenType.COMMA, "expecting comma seperation between parameters");
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
                        Consume(TokenType.COMMA, "expecting comma seperation between type arguments");
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

        return new OtherNonTerminals.Type(type);
    }

    private Statement Declaration()
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
    
    private Expr Expr()
    {
        return Assignment();
    }
    
    private Expr Assignment()
    {
        var expression = NonAssignment();
        if (!Match(TokenType.EQUAL)) return expression;
        var equals = Previous();
        var value = NonAssignment();
        return expression switch
        {
            Expr.VariableExpression variable => new Expr.AssignmentExpression(variable.name, value),
            Expr.GetFieldExpression get => new Expr.SetFieldExpression(get.obj, get.field, value),
            _ => throw new ParseException("Invalid assignment target.")
        };
    }
    
    private Expr NonAssignment()
    {
        return Peek().type switch
        {
            TokenType.LEFT_PAREN => AnonFunc(),
            TokenType.NEW => Instance(),
            _ => Or()
        };
    }
    
    private Expr AnonFunc()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' at the beginning of the anonymous function.");
        var parameters = Parameters();
        Consume(TokenType.ARROW, "Expect '=>' after parameters.");
        var body = BlockStmt();
        return new Expr.AnonFunctionExpression(parameters, body);
    }

    private Expr Instance()
    {
        Consume(TokenType.NEW, "Expect 'new' at the beginning of the instance creation.");
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier after 'new'.");
        if (!Match(TokenType.LEFT_PAREN)) throw new ParseException("Expect '(' after identifier.");
        var arguments = Arguments();
        return new Expr.InstanceExpression(identifier, arguments);
    }

    private List<Expr> Arguments()
    {
        var arguments = new List<Expr>();
        while (!Match(TokenType.RIGHT_PAREN))
        {
            arguments.Add(NonAssignment());
            if (!Match(TokenType.COMMA)) break;
        }
        return arguments;
    }

    private Expr Or()
    {
        var expression = And();

        while (Match(TokenType.OR))
        {
            var @operator = Previous();
            var right = And();
            expression = new Expr.LogicalExpression(expression, @operator, right);
        }

        return expression;
    }

    private Expr And()
    {
        var expression = Equality();

        while (Match(TokenType.AND))
        {
            var @operator = Previous();
            var right = Equality();
            expression = new Expr.LogicalExpression(expression, @operator, right);
        }

        return expression;
    }

    private Expr Equality()
    {
        var expression = Comparison();

        while (Match(TokenType.EQUAL_EQUAL) || Match(TokenType.BANG_EQUAL)) 
        {
            var @operator = Previous();
            var right = Comparison();
            expression = new Expr.BinaryExpression(expression, @operator, right);
        }

        return expression;
    }

    private Expr Comparison()
    {
        var expression = Additive();

        while(Match(TokenType.LESS) || Match(TokenType.LESS_EQUAL) || Match(TokenType.GREATER_EQUAL) || Match(TokenType.GREATER))
        {
            var @operator = Previous();
            var right = Additive();
            expression = new Expr.BinaryExpression(expression, @operator, right);
        }
        return expression;
    }

    private Expr Additive()
    {
        var expression = Multiplicative();

        while (Match(TokenType.PLUS) || Match(TokenType.MINUS))
        {
            var @operator = Previous();
            var right = Multiplicative();
            expression = new Expr.BinaryExpression(expression, @operator, right);
        }
        return expression;
    }

    private Expr Multiplicative()
    {
        var expression = Unary();

        while (Match(TokenType.STAR) || Match(TokenType.SLASH) || Match(TokenType.MOD))
        {
            var @operator = Previous();
            var right = Unary();
            expression = new Expr.BinaryExpression(expression, @operator, right);
        }
        return expression;
    }

    private Expr Unary()
    {
        if(Match(TokenType.MINUS) || Match(TokenType.BANG))
        {
            var @operator = Previous();
            var right = Unary();
            var expression = new Expr.UnaryExpression(@operator, right);
            return expression;
        }
        return Call();
    }

    private Expr Call()
    {
        var expression = Primary();

        if (expression is not Graphite.Parser.Expr.VariableExpression) return expression;

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                var arguments = Arguments();
                expression = new Expr.CallExpression(expression, arguments);
            }
            if (Match(TokenType.DOT))
            {
                var field = Consume(TokenType.IDENTIFIER, "Expect field name after '.'.");
                expression = new Expr.GetFieldExpression(expression, field);
            }
            else
            {
                break;
            }
        }
        
        return expression;
    }

    private Expr Primary()
    {
        return null;
    }

    private Expr List()
    {
        return null;
    }

    private Expr ElementAccess()
    {
        return null;
    }

    private Expr Set()
    {
        while (!Match(TokenType.RIGHT_BRACE))
        {
            Advance();
        }
        return null;
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
    
    #endregion
}