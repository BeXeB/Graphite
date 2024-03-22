using Graphite.Lexer;
using Graphite.Parser;

namespace Graphite;

public class Transpiler : Statement.IStatementVisitor<string>, Expression.IExpressionVisitor<string>
{
    public string Transpile(List<Statement> statements)
    {
        string code = "";
        foreach (var VARIABLE in statements)
        {
            code += VARIABLE.Accept(this);
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
}