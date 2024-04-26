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
        
        string variableDeclarations = "";

        foreach(var currVarDeclaration in statement.variableDeclarationStatements)
        {
            variableDeclarations += VisitVariableDeclarationStatement(currVarDeclaration.statement);
        }

        string functionDeclarations = "";

        foreach(var currFuncDeclaration in statement.functionDeclarationStatements)
        {
            functionDeclarations += VisitFunctionDeclarationStatement(currFuncDeclaration.statement);
        }

        return $"{accessModifier} {identifier} {{ {variableDeclarations} {functionDeclarations} }}";
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

    public string VisitGraphBlockStatement(GraphExpression.GraphBlockStatement expression)
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