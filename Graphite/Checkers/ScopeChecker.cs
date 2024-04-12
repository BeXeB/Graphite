using Type = Graphite.Parser.OtherNonTerminals.Type;
using Graphite.Parser;
using static Graphite.Statement;
using Graphite.Lexer;

namespace Graphite.Checkers
{
    internal class ScopeChecker :
        Statement.IStatementVisitor<object>,
        Expression.IExpressionVisitor<object>,
        OtherNonTerminals.IOtherNonTerminalsVisitor<object>,
        GraphExpression.IGraphExpressionVisitor<object>
    {
        private VariableTable variableTable;
        private FunctionTable functionTable;

        public object VisitAnonFunctionExpression(Expression.AnonFunctionExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitAssignmentExpression(Expression.AssignmentExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitBinaryExpression(Expression.BinaryExpression expression)
        {
            


            throw new NotImplementedException();
        }

        public object VisitBlockStatement(Statement.BlockStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitBreakStatement(Statement.BreakStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitCallExpression(Expression.CallExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitClassDeclarationStatement(Statement.ClassDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitContinueStatement(Statement.ContinueStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitElementAccessExpression(Expression.ElementAccessExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitExpressionStatement(Statement.ExpressionStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitFunctionDeclarationStatement(Statement.FunctionDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitGetFieldExpression(Expression.GetFieldExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphAddVertexExpression(GraphExpression.GraphAddVertexExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphBlockStmt(GraphExpression.GraphBlockStatement expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphExpressionStmt(GraphExpression.GraphExpressionStatement expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphIfStmt(GraphExpression.GraphIfStatement expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphRemoveVertexExpression(GraphExpression.GraphRemoveVertexExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphReTagExpression(GraphExpression.GraphReTagExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphStatement(Statement.GraphStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphTagExpression(GraphExpression.GraphTagExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGraphWhileStmt(GraphExpression.GraphWhileStatement expression)
        {
            throw new NotImplementedException();
        }

        public object VisitGroupingExpression(Expression.GroupingExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitIfStatement(Statement.IfStatement statement)
        {
            // Check if the condition expression is a boolean expression
            TokenType conditionType = statement.condition;
            if (conditionType != TokenType.BOOL)
            {
                throw new CheckException("Condition expression in if statement must be of type boolean.");
            }

            // Enter a new scope for the body of the if statement
            variableTable.EnterScope();
            functionTable.EnterScope();

            // Type-check the then branch of the if statement
            BlockStatement thenBranch = statement.thenBranch;
            TypeChecker.VisitBlockStatement(statement.thenBranch);

            // Exit the scope after checking the body
            variableTable.ExitScope();
            functionTable.ExitScope();

            //Checvking whether there is an else branch
            if(statement.elseBranch != null)
            {
                variableTable.EnterScope();
                functionTable.EnterScope();

                // Type-check the else branch of the if statement
                TypeChecker.VisitBlockStatement(statement.elseBranch);

                // Exit the scope after checking the body
                variableTable.ExitScope();
                functionTable.ExitScope();
            }
            throw new NotImplementedException();
        }

        public object VisitInstanceExpression(Expression.InstanceExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitListExpression(Expression.ListExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitLiteralExpression(Expression.LiteralExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitLogicalExpression(Expression.LogicalExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitParameters(OtherNonTerminals.Parameters parameters)
        {
            throw new NotImplementedException();
        }

        public object VisitPredicateAndExpression(GraphExpression.PredicateAndExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitPredicateGroupingExpression(GraphExpression.PredicateGroupingExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitPredicateLiteralExpression(GraphExpression.PredicateLiteralExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitPredicateOrExpression(GraphExpression.PredicateOrExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitPredicateUnaryExpression(GraphExpression.PredicateUnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitReturnStatement(Statement.ReturnStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitSetExpression(Expression.SetExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitSetFieldExpression(Expression.SetFieldExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitSuperExpression(Expression.SuperExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitThisExpression(Expression.ThisExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitType(OtherNonTerminals.Type type)
        {
            throw new NotImplementedException();
        }

        public object VisitUnaryExpression(Expression.UnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitVariableDeclarationStatement(Statement.VariableDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public object VisitVariableExpression(Expression.VariableExpression expression)
        {
            throw new NotImplementedException();
        }

        public object VisitWhileStatement(Statement.WhileStatement statement)
        {
            throw new NotImplementedException();
        }
    }
}
