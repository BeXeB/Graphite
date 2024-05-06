using Graphite.Lexer;
using Graphite.Parser;

namespace Graphite;

public class Transpiler :
    Statement.IStatementVisitor<string>,
    Expression.IExpressionVisitor<string>,
    OtherNonTerminals.IOtherNonTerminalsVisitor<string>,
    GraphExpression.IGraphExpressionVisitor<string>
{
    private string graphIdentifier = "";

    public string Transpile(List<Statement> statements)
    {
        var code = "using Domain; class Program { static void Main(string[] args) {";
        var classDeclarations = "";
        foreach (var statement in statements)
        {
            if (statement is Statement.ClassDeclarationStatement)
            {
                classDeclarations += statement.Accept(this);
            }
            else
            {
                code += statement.Accept(this);
            }
        }

        code += $"}} {classDeclarations} }}";

        return code;
    }

    public string VisitBlockStatement(Statement.BlockStatement statement)
    {
        var statements = statement.statements.Select(s => s.Accept(this));
        return "{" + $"{String.Join(' ', statements)}" + "}";
    }

    public string VisitExpressionStatement(Statement.ExpressionStatement statement)
    {
        var expression = statement.expression.Accept(this);
        return $"{expression};";
    }

    public string VisitIfStatement(Statement.IfStatement statement)
    {
        var condition = statement.condition.Accept(this);
        var thenBranch = statement.thenBranch.Accept(this);
        if (statement.elseBranch is null)
        {
            return $"if({condition}) {thenBranch}";
        }

        var elseBranch = statement.elseBranch.Accept(this);
        return $"if({condition}) {thenBranch} else {elseBranch}";
    }

    public string VisitWhileStatement(Statement.WhileStatement statement)
    {
        var condition = statement.condition.Accept(this);
        var body = statement.body.Accept(this);
        return $"while({condition}) {body}";
    }

    public string VisitReturnStatement(Statement.ReturnStatement statement)
    {
        var expression = statement.expression?.Accept(this);
        return $"return {expression};";
    }

    public string VisitBreakStatement(Statement.BreakStatement statement)
    {
        return "break;";
    }

    public string VisitContinueStatement(Statement.ContinueStatement statement)
    {
        return "continue;";
    }

    public string VisitGraphStatement(Statement.GraphStatement statement)
    {
        graphIdentifier = statement.identifier.lexeme;
        var code = "";
        foreach (var graphExpression in statement.expressions)
        {
            code += graphExpression.Accept(this);
        }

        graphIdentifier = "";
        return code;
    }

    public string VisitClassDeclarationStatement(Statement.ClassDeclarationStatement statement)
    {
        string accessModifier;

        switch (statement.accessModifier.lexeme)
        {
            case "public":
                accessModifier = "public";
                break;
            case "private":
                accessModifier = "private";
                break;
            default:
                throw new InvalidTokenException("Invalid or missing accessModifier");
        }

        string identifier = statement.identifier.lexeme;

        string extends = "";
        if (!(statement.extendsIdentifier is null))
        {
            extends = $" : {statement.extendsIdentifier?.lexeme}" ?? "";
        }

        string variableDeclarations = "";

        foreach (var currVarDeclaration in statement.variableDeclarationStatements)
        {
            var declaration = VisitVariableDeclarationStatement(currVarDeclaration.Item2);
            string varAccessModifier;

            switch (currVarDeclaration.Item1.lexeme)
            {
                case "public":
                    varAccessModifier = "public";
                    break;
                case "private":
                    varAccessModifier = "private";
                    break;
                default:
                    throw new InvalidTokenException("Invalid or missing accessModifier");
            }

            variableDeclarations += $"{varAccessModifier} {declaration}";
        }

        string functionDeclarations = "";

        foreach (var currFuncDeclaration in statement.functionDeclarationStatements)
        {
            var declaration = VisitFunctionDeclarationStatement(currFuncDeclaration.Item2);
            string funcAccessModifier;

            switch (currFuncDeclaration.Item1.lexeme)
            {
                case "public":
                    funcAccessModifier = "public";
                    break;
                case "private":
                    funcAccessModifier = "private";
                    break;
                default:
                    throw new InvalidTokenException("Invalid or missing accessModifier");
            }

            functionDeclarations += $"{funcAccessModifier} {declaration}";
        }

        return $"{accessModifier} class {identifier} {extends} {{ {variableDeclarations} {functionDeclarations} }}";
    }

    public string VisitFunctionDeclarationStatement(Statement.FunctionDeclarationStatement statement)
    {
        var identifier = statement.identifier.lexeme;
        var parameters = statement.parameters.Accept(this);
        var block = statement.blockStatement.Accept(this);
        var returnType = statement.returnType.Accept(this);

        return $"{returnType} {identifier}({parameters}) {block}";
    }

    public string VisitVariableDeclarationStatement(Statement.VariableDeclarationStatement statement)
    {
        var type = statement.type.Accept(this);
        var identifier = statement.identifier.lexeme;
        if (statement.initializingExpression is null)
        {
            return $"{type} {identifier};";
        }

        var expression = statement.initializingExpression.Accept(this);

        return $"{type} {identifier} = {expression};";
    }

    public string VisitBinaryExpression(Expression.BinaryExpression expression)
    {
        var left = expression.left.Accept(this);
        var right = expression.right.Accept(this);

        var @operator = expression.@operator.type switch
        {
            TokenType.EQUAL_EQUAL => "==",
            TokenType.BANG_EQUAL => "!=",
            TokenType.GREATER => ">",
            TokenType.GREATER_EQUAL => ">=",
            TokenType.LESS => "<",
            TokenType.LESS_EQUAL => "<=",
            TokenType.PLUS => "+",
            TokenType.MINUS => "-",
            TokenType.STAR => "*",
            TokenType.SLASH => "/",
            TokenType.MOD => "%",
            _ => throw new Exception("Invalid operator")
        };

        return $"{left} {@operator} {right}";
    }

    public string VisitGroupingExpression(Expression.GroupingExpression expression)
    {
        var innerExpression = expression.expression.Accept(this);
        return $"({innerExpression})";
    }

    public string VisitLiteralExpression(Expression.LiteralExpression expression)
    {
        if (expression.value is null) return "null";
        
        var a = $"{expression.value}";
        switch (expression.value)
        {
            case string:
                a = $"\"{a}\"";
                break;
            case char:
                a = $"'{a}'";
                break;
            case bool:
                a = a.ToLower();
                break;
        }

        return a;
    }

    public string VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        var right = expression.right.Accept(this);

        var @operator = expression.@operator.type switch
        {
            TokenType.MINUS => "-",
            TokenType.BANG => "!",
            _ => throw new Exception("Invalid operator")
        };

        return $"{@operator}{right}";
    }

    public string VisitAssignmentExpression(Expression.AssignmentExpression expression)
    {
        var value = expression.value.Accept(this);

        return $"{expression.name.lexeme} = {value}";
    }

    public string VisitVariableExpression(Expression.VariableExpression expression)
    {
        return expression.name.lexeme;
    }

    public string VisitLogicalExpression(Expression.LogicalExpression expression)
    {
        var left = expression.left.Accept(this);
        var right = expression.right.Accept(this);

        var @operator = expression.@operator.type switch
        {
            TokenType.AND => "&&",
            TokenType.OR => "||",
            _ => throw new Exception("Invalid operator")
        };

        return $"{left} {@operator} {right}";
    }

    public string VisitCallExpression(Expression.CallExpression expression)
    {
        var callee = expression.callee.Accept(this);
        var arguments = expression.arguments.Select(a => a.Accept(this));

        return $"{callee}({string.Join(", ", arguments)})";
    }

    public string VisitGetFieldExpression(Expression.GetFieldExpression expression)
    {
        var obj = expression.obj.Accept(this);
        var field = expression.field.Accept(this);

        return $"{obj}.{field}";
    }

    public string VisitSetFieldExpression(Expression.SetFieldExpression expression)
    {
        var obj = expression.obj.Accept(this);
        var field = expression.field.Accept(this);
        var value = expression.value.Accept(this);

        return $"{obj}.{field} = {value}";
    }

    public string VisitThisExpression(Expression.ThisExpression expression)
    {
        return "this";
    }

    public string VisitSuperExpression(Expression.SuperExpression expression)
    {
        return "base";
    }

    public string VisitListExpression(Expression.ListExpression expression)
    {
        var elements = expression.elements.Select(e => e.Accept(this));
        return $"[{string.Join(", ", elements)}]";
    }

    public string VisitSetExpression(Expression.SetExpression expression)
    {
        var elements = expression.elements.Select(e => e.Accept(this));
        return $"[{string.Join(", ", elements)}]";
    }

    public string VisitElementAccessExpression(Expression.ElementAccessExpression expression)
    {
        var obj = expression.obj.Accept(this);
        var index = expression.index.Accept(this);

        return $"{obj}[{index}]";
    }

    public string VisitInstanceExpression(Expression.InstanceExpression expression)
    {
        var arguments = expression.arguments.Select(a => a.Accept(this));

        return $"new {expression.className.lexeme}({string.Join(", ", arguments)})";
    }

    public string VisitAnonFunctionExpression(Expression.AnonFunctionExpression expression)
    {
        var body = expression.body.Accept(this);
        var parameters = expression.parameters.Accept(this);

        return $"({parameters}) => {body}";
    }

    public string VisitType(OtherNonTerminals.Type type)
    {
        switch (type.type.type)
        {
            case TokenType.IDENTIFIER:
                return type.type.lexeme;
            case TokenType.INT:
                return "int";
            case TokenType.DEC:
                return "decimal";
            case TokenType.BOOL:
                return "bool";
            case TokenType.STR:
                return "string";
            case TokenType.CHAR:
                return "char";
            case TokenType.VOID:
                return "void";
            case TokenType.SET:
                var setType = type.typeArguments[0].Accept(this);
                return $"HashSet<{setType}>";
            case TokenType.LIST:
                var listType = type.typeArguments[0].Accept(this);
                return $"List<{listType}>";
            case TokenType.FUNC_TYPE:
                var returnType = type.typeArguments[0].Accept(this);
                var parameters = String.Join(',', type.typeArguments.Skip(1).Select(t => t.Accept(this)));
                return returnType.Equals("void") ? $"Action<{parameters}>" :
                    parameters.Length > 0 ? $"Func<{parameters}, {returnType}>" : $"Func<{returnType}>";
            default:
                throw new TranspileException("Invalid type in transpiler. At: " + type.type.line);
        }
    }

    public string VisitParameters(OtherNonTerminals.Parameters parameters)
    {
        var parameter = parameters.parameters.Select((t) => $"{t.Item1.Accept(this)} {t.Item2.lexeme}");
        return String.Join(',', parameter);
    }

    #region GraphStatement

    public string VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
    {
        var leftPredicate = $"v => {expression.leftPredicate.Accept(this)}";
        var rightPredicate = $"v => {expression.rightPredicate.Accept(this)}";
        var weight = expression.weight.Accept(this);
        switch (expression.@operator.type)
        {
            case TokenType.ARROW:
                return $"{graphIdentifier}.Connect({leftPredicate}, {rightPredicate}, {weight});";
            case TokenType.DOUBLE_ARROW:
                return $"{graphIdentifier}.Connect({leftPredicate}, {rightPredicate}, {weight});" +
                       $"{graphIdentifier}.Connect({rightPredicate}, {leftPredicate}, {weight});";
            case TokenType.SLASHED_EQUAL:
                return $"{graphIdentifier}.Disconnect({leftPredicate}, {rightPredicate});";
            default:
                throw new TranspileException("Invalid operator in graph edge expression. At: "
                                             + expression.@operator.line);
        }
    }

    public string VisitGraphAddVertexExpression(GraphExpression.GraphAddVertexExpression expression)
    {
        var tags = expression.tags.Accept(this);
        var times = expression.times.Accept(this);
        return $"{graphIdentifier}.AddVertex({tags}, {times});";
    }

    public string VisitGraphRemoveVertexExpression(GraphExpression.GraphRemoveVertexExpression expression)
    {
        var predicate = $"v => {expression.predicate.Accept(this)}";
        return $"{graphIdentifier}.RemoveVertex({predicate});";
    }

    public string VisitGraphTagExpression(GraphExpression.GraphTagExpression expression)
    {
        var predicate = $"v => {expression.predicate.Accept(this)}";
        var tags = expression.tags.Accept(this);
        switch (expression.@operator.type)
        {
            case TokenType.PLUS_PLUS:
                return $"{graphIdentifier}.AddTags({predicate}, {tags});";
            case TokenType.MINUS_MINUS:
                return $"{graphIdentifier}.RemoveTags({predicate}, {tags});";
            default:
                throw new TranspileException("Invalid operator in graph tag expression. At: "
                                             + expression.@operator.line);
        }
    }

    public string VisitGraphReTagExpression(GraphExpression.GraphReTagExpression expression)
    {
        var oldTag = expression.oldTag.Accept(this);
        var newTag = expression.newTag.Accept(this);
        return $"{graphIdentifier}.Retag({oldTag}, {newTag});";
    }

    public string VisitGraphWhileStmt(GraphExpression.GraphWhileStatement expression)
    {
        var condition = expression.condition.Accept(this);
        var code = $"while ({condition})";
        code += expression.body.Accept(this);
        return code;
    }

    public string VisitGraphIfStmt(GraphExpression.GraphIfStatement expression)
    {
        var condition = expression.condition.Accept(this);
        var code = $"if ({condition})";
        code += expression.thenBranch.Accept(this);
        if (expression.elseBranch == null) return code;
        code += "else";
        code += expression.elseBranch.Accept(this);
        return code;
    }

    public string VisitGraphExpressionStmt(GraphExpression.GraphExpressionStatement expression)
    {
        return expression.statement.Accept(this);
    }

    public string VisitGraphBlockStatement(GraphExpression.GraphBlockStatement expression)
    {
        var code = "{";
        foreach (var graphExpression in expression.statements)
        {
            code += graphExpression.Accept(this);
        }

        code += "}";
        return code;
    }

    public string VisitAddGraphExpression(GraphExpression.AddGraphExpression expression)
    {
        return $"{graphIdentifier}.AddGraph({expression.otherGraph.lexeme})";
    }

    public string VisitPredicateOrExpression(GraphExpression.PredicateOrExpression expression)
    {
        var left = expression.left.Accept(this);
        var right = expression.right.Accept(this);
        return $"{left} || {right}";
    }

    public string VisitPredicateAndExpression(GraphExpression.PredicateAndExpression expression)
    {
        var left = expression.left.Accept(this);
        var right = expression.right.Accept(this);
        return $"{left} && {right}";
    }

    public string VisitPredicateGroupingExpression(GraphExpression.PredicateGroupingExpression expression)
    {
        var expressionCode = expression.expression.Accept(this);
        return $"({expressionCode})";
    }

    public string VisitPredicateUnaryExpression(GraphExpression.PredicateUnaryExpression expression)
    {
        var right = expression.right.Accept(this);
        return $"!{right}";
    }

    public string VisitPredicateLiteralExpression(GraphExpression.PredicateLiteralExpression expression)
    {
        var literal = expression.expression.Accept(this);
        return $"v.Contains({literal})";
    }

    #endregion
}