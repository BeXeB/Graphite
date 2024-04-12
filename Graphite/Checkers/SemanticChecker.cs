using Graphite.Parser;

namespace Graphite.Checkers
{
    internal class SemanticChecker :
        Statement.IStatementVisitor<OtherNonTerminals.Type>,
        Expression.IExpressionVisitor<OtherNonTerminals.Type>,
        OtherNonTerminals.IOtherNonTerminalsVisitor<OtherNonTerminals.Type>,
        GraphExpression.IGraphExpressionVisitor<OtherNonTerminals.Type>
    {
        public OtherNonTerminals.Type VisitAnonFunctionExpression(Expression.AnonFunctionExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitAssignmentExpression(Expression.AssignmentExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitBinaryExpression(Expression.BinaryExpression expression)
        {
            expression.left.Accept


            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitBlockStatement(Statement.BlockStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitBreakStatement(Statement.BreakStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitCallExpression(Expression.CallExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitClassDeclarationStatement(Statement.ClassDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitContinueStatement(Statement.ContinueStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitElementAccessExpression(Expression.ElementAccessExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitExpressionStatement(Statement.ExpressionStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitFunctionDeclarationStatement(Statement.FunctionDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGetFieldExpression(Expression.GetFieldExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphAddVertexExpression(GraphExpression.GraphAddVertexExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphBlockStmt(GraphExpression.GraphBlockStatement expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphExpressionStmt(GraphExpression.GraphExpressionStatement expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphIfStmt(GraphExpression.GraphIfStatement expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphRemoveVertexExpression(GraphExpression.GraphRemoveVertexExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphReTagExpression(GraphExpression.GraphReTagExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphStatement(Statement.GraphStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphTagExpression(GraphExpression.GraphTagExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGraphWhileStmt(GraphExpression.GraphWhileStatement expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitGroupingExpression(Expression.GroupingExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitIfStatement(Statement.IfStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitInstanceExpression(Expression.InstanceExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitListExpression(Expression.ListExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitLiteralExpression(Expression.LiteralExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitLogicalExpression(Expression.LogicalExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitParameters(OtherNonTerminals.Parameters parameters)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitPredicateAndExpression(GraphExpression.PredicateAndExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitPredicateGroupingExpression(GraphExpression.PredicateGroupingExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitPredicateLiteralExpression(GraphExpression.PredicateLiteralExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitPredicateOrExpression(GraphExpression.PredicateOrExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitPredicateUnaryExpression(GraphExpression.PredicateUnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitReturnStatement(Statement.ReturnStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitSetExpression(Expression.SetExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitSetFieldExpression(Expression.SetFieldExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitSuperExpression(Expression.SuperExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitThisExpression(Expression.ThisExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitType(OtherNonTerminals.Type type)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitUnaryExpression(Expression.UnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitVariableDeclarationStatement(Statement.VariableDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitVariableExpression(Expression.VariableExpression expression)
        {
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitWhileStatement(Statement.WhileStatement statement)
        {
            throw new NotImplementedException();
        }
    }
}
