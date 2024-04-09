using Graphite.Lexer;
using Graphite.Parser;

namespace Graphite;

public class Transpiler : 
    Statement.IStatementVisitor<string>, 
    Expression.IExpressionVisitor<string>, 
    OtherNonTerminals.IOtherNonTerminalsVisitor<string>, 
    GraphExpression.IGraphExpressionVisitor<string>
{
    public string Transpile(List<Statement> statements)
    {
        string code = "";
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
        throw new NotImplementedException();
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
            ">" => ">",
            ">=" => ">=",
            "<" => "<",
            "<=" => "<=",
            "+" => "+",
            "-" => "-",
            "*" => "*",
            "/" => "/",
            "mod" => "%",
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
        return $"{expression.value}";
    }

    public string VisitUnaryExpression(Expression.UnaryExpression expression)
    {
        var right = expression.right.Accept(this);

        var @operator = expression.@operator.lexeme switch
        {
            "-" => "-",
            "!" => "!",
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

        var @operator = expression.@operator.lexeme switch
        {
            "and" => "&&",
            "or" => "||",
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
        return "super";
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
        throw new NotImplementedException();
    }

    public string VisitParameters(OtherNonTerminals.Parameters parameters)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphAddVertexExpression(GraphExpression.GraphAddVertexExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphRemoveVertexExpression(GraphExpression.GraphRemoveVertexExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphTagExpression(GraphExpression.GraphTagExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphReTagExpression(GraphExpression.GraphReTagExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphWhileStmt(GraphExpression.GraphWhileStatement expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphIfStmt(GraphExpression.GraphIfStatement expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphExpressionStmt(GraphExpression.GraphExpressionStatement expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGraphBlockStmt(GraphExpression.GraphBlockStatement expression)
    {
        throw new NotImplementedException();
    }

    public string VisitPredicateOrExpression(GraphExpression.PredicateOrExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitPredicateAndExpression(GraphExpression.PredicateAndExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitPredicateGroupingExpression(GraphExpression.PredicateGroupingExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitPredicateUnaryExpression(GraphExpression.PredicateUnaryExpression expression)
    {
        throw new NotImplementedException();
    }

    public string VisitPredicateLiteralExpression(GraphExpression.PredicateLiteralExpression expression)
    {
        throw new NotImplementedException();
    }
}