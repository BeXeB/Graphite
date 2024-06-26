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
                Console.WriteLine(e.Message);
                return statements;
            }
        }

        return statements;
    }

    #region Statements

    private Statement Declaration()
    {
        var token = Peek();
        switch (token.type)
        {
            case TokenType.INT:
            case TokenType.DEC:
            case TokenType.BOOL:
            case TokenType.STR:
            case TokenType.CHAR:
            case TokenType.FUNC_TYPE:
            case TokenType.SET:
            case TokenType.LIST:
                return VariableDeclarationStatement();
            case TokenType.PUBLIC:
            case TokenType.PRIVATE:
                return ClassDeclarationStatement();
            case TokenType.IDENTIFIER:
                switch (Peek(1).type)
                {
                    case TokenType.LEFT_PAREN:
                        var i = 1;
                        while (true)
                        {
                            if (Peek(i).type == TokenType.RIGHT_PAREN) break;
                            i++;
                        }

                        return Peek(i + 1).type == TokenType.RETURNS ? FunctionDeclarationStatement() : Statement();
                    case TokenType.IDENTIFIER:
                        return VariableDeclarationStatement();
                    default:
                        return Statement();
                }
            default:
                return Statement();
        }
    }

    private ClassDeclarationStatement ClassDeclarationStatement()
    {
        Token accessModifier;
        Token classIdentifier;
        Token? extendsIdentifier = null;
        List<(Token accesModifier, VariableDeclarationStatement statement)> variableDeclarationStatements = [];
        List<(Token accesModifier, FunctionDeclarationStatement statement)> functionDeclarationStatements = [];

        accessModifier = Peek();

        if (accessModifier.type != TokenType.PUBLIC && accessModifier.type != TokenType.PRIVATE)
        {
            throw new ParseException("Invalid or missing access modifier", Peek());
        }

        Advance();

        Consume(TokenType.CLASS, "Expecting 'class' at the beginning of the class declaration");

        classIdentifier = Consume(TokenType.IDENTIFIER, "Invalid class identifier");

        if (Peek().type == TokenType.EXTENDS)
        {
            Consume(TokenType.EXTENDS, "Unexpected internal error in parser");
            extendsIdentifier = Consume(TokenType.IDENTIFIER, "Extending invalid identifier");
        }

        Consume(TokenType.LEFT_BRACE, "Expecting '{' after identifier at class declaration");

        while (Peek().type != TokenType.RIGHT_BRACE)
        {
            var modifier = Peek();

            if (accessModifier.type != TokenType.PUBLIC && accessModifier.type != TokenType.PRIVATE)
            {
                throw new ParseException("Invalid or missing access modifier", Peek());
            }

            Advance();

            switch (Peek(1).type)
            {
                case TokenType.LEFT_PAREN: // Meaning it is a function declaration
                    functionDeclarationStatements.Add((modifier, FunctionDeclarationStatement()));
                    break;
                case TokenType.IDENTIFIER: // Meaning it is a variable declaration
                    variableDeclarationStatements.Add((modifier, VariableDeclarationStatement()));
                    break;
                default:
                    throw new ParseException(
                        "Unexpected token inside class declaration. Expected class- or function declaration", Peek(1));
            }
        }

        Consume(TokenType.RIGHT_BRACE, "Expecting '}' after class declaration");

        return new ClassDeclarationStatement(
            accessModifier,
            classIdentifier,
            extendsIdentifier,
            variableDeclarationStatements,
            functionDeclarationStatements,
            classIdentifier.line
        );
    }

    private VariableDeclarationStatement VariableDeclarationStatement()
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

        return new VariableDeclarationStatement(type, identifier, initializingExpression, identifier.line);
    }

    private FunctionDeclarationStatement FunctionDeclarationStatement()
    {
        Token identifier;
        OtherNonTerminals.Parameters parameters;
        BlockStatement blockStatement;

        identifier = Consume(TokenType.IDENTIFIER, "expecting function identifier");

        parameters = Parameters();

        Consume(TokenType.RETURNS, "expecting 'returns' after function parameters");

        var peek = Peek();
        var returnType = peek.type == TokenType.VOID ? new OtherNonTerminals.Type(Advance(), []) : Type();

        blockStatement = BlockStatement();

        return new FunctionDeclarationStatement(identifier, parameters, blockStatement, returnType, identifier.line);
    }

    private OtherNonTerminals.Parameters Parameters()
    {
        var lineNumber = Peek().line;
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

        Consume(TokenType.RIGHT_PAREN, "missing right parentheses after parameters");

        return new OtherNonTerminals.Parameters(parameters, lineNumber);
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
            case TokenType.SET:
            case TokenType.LIST:
                Advance();
                Consume(TokenType.LESS, "expected '<' after Set type to declare type arguments");
                typeArguments.Add(Type());
                Consume(TokenType.GREATER, "expecting type arguments to end with an >");
                break;
            case TokenType.FUNC_TYPE:
                Advance();
                Consume(TokenType.LESS, "expected '<' after Func type to declare return- and argument types");

                bool firstArgument = true;

                while (Peek().type != TokenType.GREATER)
                {
                    if (!firstArgument)
                    {
                        Consume(TokenType.COMMA, "expecting comma separation between type arguments");
                    }

                    if (Peek().type == TokenType.VOID && firstArgument)
                    {
                        typeArguments.Add(new OtherNonTerminals.Type(Advance(), []));
                        firstArgument = false;
                        continue;
                    }

                    OtherNonTerminals.Type argumentType = Type();
                    typeArguments.Add(argumentType);
                    firstArgument = false;
                }

                Consume(TokenType.GREATER, "expecting type arguments to end with an >");
                break;
            default:
                throw new ParseException("Invalid or missing type", Peek());
        }

        return new OtherNonTerminals.Type(type, typeArguments);
    }

    private Statement Statement()
    {
        var token = Peek();
        return token.type switch
        {
            TokenType.IF => IfStatement(),
            TokenType.WHILE => WhileStatement(),
            TokenType.RETURN => ReturnStatement(),
            TokenType.BREAK => BreakStatement(),
            TokenType.CONTINUE => ContinueStatement(),
            TokenType.IDENTIFIER => Peek(1).type == TokenType.LEFT_BRACE ? GraphStatement() : ExpressionStatement(),
            TokenType.LEFT_BRACE => BlockStatement(),
            _ => ExpressionStatement()
        };
    }

    private IfStatement IfStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.IF, "Expect 'if' at the beginning of the statement");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");
        var thenBranch = BlockStatement();
        if (Match(TokenType.ELSE))
        {
            var elseBranch = BlockStatement();
            return new IfStatement(condition, thenBranch, elseBranch, lineNumber);
        }

        return new IfStatement(condition, thenBranch, null, lineNumber);
    }

    private WhileStatement WhileStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.WHILE, "Expect 'while' at the beginning of the statement");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");
        var body = BlockStatement();
        return new WhileStatement(condition, body, lineNumber);
    }

    private ReturnStatement ReturnStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.RETURN, "Expect 'return' at the beginning of the statement");
        if (Match(TokenType.SEMICOLON))
            return new ReturnStatement(null, lineNumber);
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement");
        return new ReturnStatement(value, lineNumber);
    }

    private BreakStatement BreakStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.BREAK, "Expect 'break' at the beginning of the statement");
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement");
        return new BreakStatement(lineNumber);
    }

    private ContinueStatement ContinueStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.CONTINUE, "Expect 'continue' at the beginning of the statement");
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement");
        return new ContinueStatement(lineNumber);
    }

    private BlockStatement BlockStatement()
    {
        var lineNumber = Peek().line;
        var statements = new List<Statement>();

        Consume(TokenType.LEFT_BRACE, "Expect '{' at the beginning of the block");

        while (!Match(TokenType.RIGHT_BRACE))
        {
            statements.Add(Declaration());
        }

        return new BlockStatement(statements, lineNumber);
    }

    private Statement ExpressionStatement()
    {
        var expression = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement");
        return new ExpressionStatement(expression, expression.Line);
    }

    #endregion

    #region GraphStatement

    private GraphStatement GraphStatement()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier");
        if (!Match(TokenType.LEFT_BRACE))
            throw new ParseException("Expect '{' after identifier", Peek());
        var expressions = new List<GraphExpression>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            expressions.Add(GraphOperation());
        }

        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the statement");
        return new GraphStatement(identifier, expressions, identifier.line);
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
            case TokenType.WHILE:
                return GraphWhileStatement();
            case TokenType.IF:
                return GraphIfStatement();
            case TokenType.PLUS_PLUS:
                return AddGraph();
            default:
                var i = 1;
                while (true)
                {
                    var peekType = Peek(i).type;
                    if (peekType is TokenType.LEFT_LEFT or TokenType.SEMICOLON) break;
                    i++;
                }

                return Peek(i).type == TokenType.LEFT_LEFT ? RetagOperation() : GraphExpressionStatement();
        }
    }

    private GraphExpression VertexOperation()
    {
        Consume(TokenType.V, "Expect 'V' at the beginning of the expression");
        var token = Peek();
        switch (token.type)
        {
            case TokenType.PLUS:
                return GraphAddVertexExpression();
            case TokenType.MINUS:
                return GraphRemoveVertexExpression();
            default:
                throw new ParseException("Expect '+' or '-' after 'V'", token);
        }
    }

    private GraphExpression.GraphAddVertexExpression GraphAddVertexExpression()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.PLUS, "Expect '+' after 'V'");
        var tags = Set();
        var expression = Peek().type != TokenType.SEMICOLON
            ? new GraphExpression.GraphAddVertexExpression(tags, Expression(), lineNumber)
            : new GraphExpression.GraphAddVertexExpression(tags,
                new Expression.LiteralExpression(1, new Token { type = TokenType.INT_LITERAL }, lineNumber),
                lineNumber);
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression");
        return expression;
    }

    private GraphExpression.GraphRemoveVertexExpression GraphRemoveVertexExpression()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.MINUS, "Expect '-' after 'V'");
        var predicate = Predicate();
        var expression = new GraphExpression.GraphRemoveVertexExpression(predicate, lineNumber);
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression");
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
                Consume(token.type, "Expect '=>', '<=>' or '=/=' after predicate");
                var right = Predicate();
                var peek = Peek();
                var weight = peek.type is not TokenType.SEMICOLON
                    ? Expression()
                    : new Expression.LiteralExpression(1, new Token { type = TokenType.INT_LITERAL }, token.line);
                Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression");
                return new GraphExpression.GraphEdgeExpression(predicate, token, right, weight, token.line);
            case TokenType.PLUS_PLUS:
            case TokenType.MINUS_MINUS:
                Consume(token.type, "Expect '++' or '--' after predicate");
                var tags = Set();
                Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression");
                return new GraphExpression.GraphTagExpression(predicate, token, tags, token.line);
            default:
                throw new ParseException("Expect '=>', '<=>', '=/=', '++' or '--' after predicate",
                    token);
        }
    }

    private GraphExpression AddGraph()
    {
        Consume(TokenType.PLUS_PLUS, "Expect '++' at the beginning of the expression");
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier after '++'");
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression");
        return new GraphExpression.AddGraphExpression(identifier, identifier.line);
    }

    private GraphExpression Predicate()
    {
        Consume(TokenType.LEFT_BRACKET, "Expect '[' at the beginning of a predicate");
        var graphPredicate = PredicateOr();
        Consume(TokenType.RIGHT_BRACKET, "Expect ']' at the end of a predicate");
        return graphPredicate;
    }

    private GraphExpression PredicateOr()
    {
        var expression = PredicateAnd();
        while (Match(TokenType.OR))
        {
            var @operator = Previous();
            var right = PredicateAnd();
            expression = new GraphExpression.PredicateOrExpression(expression, @operator, right, @operator.line);
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
            expression = new GraphExpression.PredicateAndExpression(expression, @operator, right, @operator.line);
        }

        return expression;
    }

    private GraphExpression PredicateUnary()
    {
        if (Match(TokenType.BANG))
        {
            var @operator = Previous();
            var right = PredicateUnary();
            return new GraphExpression.PredicateUnaryExpression(@operator, right, @operator.line);
        }

        return PredicateLiteral();
    }

    private GraphExpression PredicateLiteral()
    {
        var lineNumber = Peek().line;
        if (Match(TokenType.LEFT_PAREN))
        {
            var expression = PredicateOr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression");
            return new GraphExpression.PredicateGroupingExpression(expression, lineNumber);
        }

        var stringExpression = Additive();
        return new GraphExpression.PredicateLiteralExpression(stringExpression, lineNumber);
    }

    private GraphExpression.GraphReTagExpression RetagOperation()
    {
        var oldTag = NonAssignment();
        Consume(TokenType.LEFT_LEFT, "Expect '<<' after string literal");
        var newTag = NonAssignment();
        Consume(TokenType.SEMICOLON, "Expect ';' at the end of the expression");
        return new GraphExpression.GraphReTagExpression(oldTag, newTag, oldTag.Line);
    }

    private GraphExpression.GraphWhileStatement GraphWhileStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.WHILE, "Expect 'while' at the beginning of the statement");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");
        var body = GraphBlockStatement();
        return new GraphExpression.GraphWhileStatement(condition, body, lineNumber);
    }

    private GraphExpression.GraphIfStatement GraphIfStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.IF, "Expect 'if' at the beginning of the statement");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");
        var thenBranch = GraphBlockStatement();
        if (Match(TokenType.ELSE))
        {
            var elseBranch = GraphBlockStatement();
            return new GraphExpression.GraphIfStatement(condition, thenBranch, elseBranch, lineNumber);
        }

        return new GraphExpression.GraphIfStatement(condition, thenBranch, null, lineNumber);
    }

    private GraphExpression.GraphBlockStatement GraphBlockStatement()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.LEFT_BRACE, "Expect '{' at the beginning of the block");
        var statements = new List<GraphExpression>();
        while (!Match(TokenType.RIGHT_BRACE))
        {
            statements.Add(GraphOperation());
        }

        return new GraphExpression.GraphBlockStatement(statements, lineNumber);
    }

    private GraphExpression.GraphExpressionStatement GraphExpressionStatement()
    {
        var statement = ExpressionStatement();
        return new GraphExpression.GraphExpressionStatement(statement, statement.Line);
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
        var lineNumber = Peek().line;
        if (!Match(TokenType.EQUAL)) return expression;
        var value = NonAssignment();
        return expression switch
        {
            Expression.VariableExpression variable => new Expression.AssignmentExpression(variable.name, value,
                lineNumber),
            Expression.GetFieldExpression get => new Expression.SetFieldExpression(get.obj, get.field, value,
                lineNumber),
            _ => throw new ParseException("Invalid assignment target", Peek())
        };
    }

    private Expression NonAssignment()
    {
        var peek = Peek();

        switch (peek.type)
        {
            case TokenType.NEW:
                return Instance();
            case TokenType.LEFT_PAREN:
                var i = 1;
                while (true)
                {
                    if (Peek(i).type == TokenType.RIGHT_PAREN) break;
                    i++;
                }

                return Peek(i + 1).type == TokenType.ARROW ? AnonymousFunction() : Or();
            default:
                return Or();
        }
    }

    private Expression AnonymousFunction()
    {
        var parameters = Parameters();
        Consume(TokenType.ARROW, "Expect '=>' after parameters");
        var body = BlockStatement();
        return new Expression.AnonFunctionExpression(parameters, body, parameters.Line);
    }

    private Expression Instance()
    {
        Consume(TokenType.NEW, "Expect 'new' at the beginning of the instance creation");
        var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier after 'new'");
        if (!Match(TokenType.LEFT_PAREN))
            throw new ParseException("Expect '(' after identifier", Peek());
        var arguments = Arguments();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments");
        return new Expression.InstanceExpression(identifier, arguments, identifier.line);
    }

    private List<Expression> Arguments()
    {
        var arguments = new List<Expression>();
        if (Peek().type == TokenType.RIGHT_PAREN) return arguments;
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
            expression = new Expression.LogicalExpression(expression, @operator, right, @operator.line);
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
            expression = new Expression.LogicalExpression(expression, @operator, right, @operator.line);
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
            expression = new Expression.BinaryExpression(expression, @operator, right, @operator.line);
        }

        return expression;
    }

    private Expression Comparison()
    {
        var expression = Additive();

        while (Match(TokenType.LESS) || Match(TokenType.LESS_EQUAL) || Match(TokenType.GREATER_EQUAL) ||
               Match(TokenType.GREATER))
        {
            var @operator = Previous();
            var right = Additive();
            expression = new Expression.BinaryExpression(expression, @operator, right, @operator.line);
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
            expression = new Expression.BinaryExpression(expression, @operator, right, @operator.line);
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
            expression = new Expression.BinaryExpression(expression, @operator, right, @operator.line);
        }

        return expression;
    }

    private Expression Unary()
    {
        if (Match(TokenType.MINUS) || Match(TokenType.BANG))
        {
            var @operator = Previous();
            var right = Unary();
            var expression = new Expression.UnaryExpression(@operator, right, @operator.line);
            return expression;
        }

        return Call();
    }

    private Expression Call()
    {
        var expression = Primary();

        while (true)
        {
            var lineNumber = Peek().line;
            if (Match(TokenType.LEFT_PAREN))
            {
                var arguments = Arguments();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments");
                expression = new Expression.CallExpression(expression, arguments, lineNumber);
            }
            else if (Match(TokenType.LEFT_BRACKET))
            {
                var index = NonAssignment();
                Consume(TokenType.RIGHT_BRACKET, "Expect ']' after index");
                expression = new Expression.ElementAccessExpression(expression, index, lineNumber);
            }
            else if (Match(TokenType.DOT))
            {
                var field = Call();
                expression = new Expression.GetFieldExpression(expression, field, lineNumber);
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
        Token token = Peek();

        switch (token.type)
        {
            case TokenType.LEFT_PAREN:
                Advance();
                var expression = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments");
                return new Expression.GroupingExpression(expression, token.line);
            case TokenType.LEFT_BRACE:
                return Set();
            case TokenType.LEFT_BRACKET:
                return List();
            case TokenType.STRING_LITERAL:
            case TokenType.CHAR_LITERAL:
            case TokenType.INT_LITERAL:
            case TokenType.DECIMAL_LITERAL:
                var literal = Advance().literal;
                return new Expression.LiteralExpression(literal, token, token.line);
            case TokenType.TRUE:
                Advance();
                return new Expression.LiteralExpression(true, token, token.line);
            case TokenType.FALSE:
                Advance();
                return new Expression.LiteralExpression(false, token, token.line);
            case TokenType.NULL:
                Advance();
                return new Expression.LiteralExpression(null, token, token.line);
            case TokenType.IDENTIFIER:
                Advance();
                return new Expression.VariableExpression(token, token.line);
            case TokenType.THIS:
                Advance();
                return new Expression.ThisExpression(token.line);
            case TokenType.SUPER:
                Advance();
                return new Expression.SuperExpression(token.line);
            default:
                throw new ParseException("Unexpected expression", token);
        }
    }

    private Expression Set()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.LEFT_BRACE, "Expect '{' at the beginning of the set");
        var elements = Arguments();
        Consume(TokenType.RIGHT_BRACE, "Expect '}' at the end of the set");
        return new Expression.SetExpression(elements, lineNumber);
    }

    private Expression List()
    {
        var lineNumber = Peek().line;
        Consume(TokenType.LEFT_BRACKET, "Expect '[' at the beginning of the list");
        var elements = Arguments();
        Consume(TokenType.RIGHT_BRACKET, "Expect ']' at the end of the list");
        return new Expression.ListExpression(elements, lineNumber);
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
        throw new ParseException(errorMessage, Peek());
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