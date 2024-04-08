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
        var code = "";
        foreach (var statement in statements)
        {
            code += statement.Accept(this);
        }

        return code;
    }
    
    public string VisitBlockStatement(Statement.BlockStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitExpressionStatement(Statement.ExpressionStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitIfStatement(Statement.IfStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitWhileStatement(Statement.WhileStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitReturnStatement(Statement.ReturnStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitBreakStatement(Statement.BreakStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitContinueStatement(Statement.ContinueStatement statement)
    {
        throw new NotImplementedException();
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
                throw new InvalidTokenException("Invalid or missing accessmodifier");
        }

        string identifier = statement.identifier.lexeme;

        string extends = statement.extends?.lexeme ?? "";
        string extendsIdentifier = statement.extends?.lexeme ?? "";

        string variableDeclarations = "";

        foreach(var currVarDeclaration in statement.variableDeclarationStatements)
        {
            variableDeclarations += VisitVariableDeclarationStatement(currVarDeclaration);
        }

        string functionDeclarations = "";

        foreach(var currFuncDeclaration in statement.functionDeclarationStatements)
        {
            functionDeclarations += VisitFunctionDeclarationStatement(currFuncDeclaration);
        }

        return $"{accessModifier} {identifier} {extends} {extendsIdentifier} {{ {variableDeclarations} {functionDeclarations} }}";
    }

    public string VisitFunctionDeclarationStatement(Statement.FunctionDeclarationStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitVariableDeclarationStatement(Statement.VariableDeclarationStatement statement)
    {
        throw new NotImplementedException();
    }

    public string VisitBinaryExpression(Expression.BinaryExpression expression)
    {
        var left = expression.left.Accept(this);
        var right = expression.right.Accept(this);
        
        var @operator = expression.@operator.lexeme switch
        {
            "==" => "==",
            "!=" => "!=",
            _ => throw new Exception("Invalid operator")
        };
        
        return $"{left} {@operator} {right}";
    }

    public string VisitGroupingExpression(Expression.GroupingExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitLiteralExpression(Expression.LiteralExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitAssignmentExpression(Expression.AssignmentExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitVariableExpression(Expression.VariableExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitLogicalExpression(Expression.LogicalExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitCallExpression(Expression.CallExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGetFieldExpression(Expression.GetFieldExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitSetFieldExpression(Expression.SetFieldExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitThisExpression(Expression.ThisExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitSuperExpression(Expression.SuperExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitListExpression(Expression.ListExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitSetExpression(Expression.SetExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitElementAccessExpression(Expression.ElementAccessExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitInstanceExpression(Expression.InstanceExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitAnonFunctionExpression(Expression.AnonFunctionExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitType(OtherNonTerminals.Type type)
    {
        if (type.type is null) return "";
        switch (type.type.Value.type)
        {
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
                var parameters = "";
                for (var i = 1; i < type.typeArguments.Count; i++)
                {
                    parameters += type.typeArguments[i].Accept(this) + ", ";
                }
                parameters = parameters.Remove(parameters.Length - 2);
                return returnType.Equals("void") ? $"Action<{parameters}>" : $"Func<{parameters}, {returnType}>";
            default:
                throw new TranspileException("Invalid type in transpiler. At: " + type.type.Value.line);
        }
    }

    public string VisitParameters(OtherNonTerminals.Parameters parameters)
    {
        var code = "";
        foreach (var (type, name) in parameters.parameters)
        {
            code += type.Accept(this) + " " + name.lexeme + ", ";
        }
        code = code.Remove(code.Length - 2);
        return code;
    }

    #region GraphStatement
    
    public string VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
    {
        var leftPredicate = $"v => {expression.leftPredicate.Accept(this)}"; 
        var rightPredicate = $"v => {expression.rightPredicate.Accept(this)}";
        var weight = expression.weight.Accept(this);
        //TODO: Implement weight in Graph Classes
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
        var oldTag = expression.oldTag.lexeme;
        var newTag = expression.newTag.lexeme;
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

    public string VisitGraphBlockStmt(GraphExpression.GraphBlockStatement expression)
    {
        var code = "{";
        foreach (var graphExpression in expression.statements)
        {
            code += graphExpression.Accept(this);
        }
        code += "}";
        return code;
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