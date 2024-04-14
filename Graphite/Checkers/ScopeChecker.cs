using Type = Graphite.Parser.OtherNonTerminals.Type;
using Graphite.Parser;
using static Graphite.Statement;
using Graphite.Lexer;

namespace Graphite.Checkers
{
    internal class ScopeChecker :
        Statement.IStatementVisitor<Type>,
        Expression.IExpressionVisitor<Type>,
        OtherNonTerminals.IOtherNonTerminalsVisitor<Type>,
        GraphExpression.IGraphExpressionVisitor<Type>
    {
        private VariableTable variableTable;
        private FunctionTable functionTable;

        public Type VisitAnonFunctionExpression(Expression.AnonFunctionExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitAssignmentExpression(Expression.AssignmentExpression expression)
        {
            if (!variableTable.IsVariableDeclared(expression.name.lexeme))
            {
                throw new CheckException("Variable has not been declared.");
            }

            var valueType = expression.value.Accept(this);
            var variableType = variableTable.GetVariableType(expression.name.lexeme);

            if (valueType != variableType)
            {
                throw new CheckException($"Cannot convert {valueType} to {variableType}.");
            }

            //TO DO: create a check also for the name

            throw new NotImplementedException();
        }

        public Type VisitBinaryExpression(Expression.BinaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitBlockStatement(Statement.BlockStatement statement)
        {
            // Enter a new scope for the body of the if statement
            variableTable.EnterScope();
            functionTable.EnterScope();

            foreach (var singleStatement in statement.statements)
            {
                singleStatement.Accept(this);
            }

            // Exit the scope after checking the body
            variableTable.ExitScope();
            functionTable.ExitScope();
            throw new NotImplementedException();
        }

        public Type VisitBreakStatement(Statement.BreakStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitCallExpression(Expression.CallExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitClassDeclarationStatement(Statement.ClassDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitContinueStatement(Statement.ContinueStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitElementAccessExpression(Expression.ElementAccessExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitExpressionStatement(Statement.ExpressionStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitFunctionDeclarationStatement(Statement.FunctionDeclarationStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitGetFieldExpression(Expression.GetFieldExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphAddVertexExpression(GraphExpression.GraphAddVertexExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphBlockStmt(GraphExpression.GraphBlockStatement expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphExpressionStmt(GraphExpression.GraphExpressionStatement expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphIfStmt(GraphExpression.GraphIfStatement expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphRemoveVertexExpression(GraphExpression.GraphRemoveVertexExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphReTagExpression(GraphExpression.GraphReTagExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphStatement(Statement.GraphStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphTagExpression(GraphExpression.GraphTagExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphWhileStmt(GraphExpression.GraphWhileStatement expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGroupingExpression(Expression.GroupingExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitIfStatement(Statement.IfStatement statement)
        {
            // Check if the condition expression is a boolean expression
            TokenType conditionType = statement.condition.Accept(this).type.Value.type;
            if (conditionType != TokenType.BOOL)
            {
                throw new CheckException("Condition expression in if statement must be of type boolean.");
            }

            // Type-check the then branch of the if statement
            statement.thenBranch.Accept(this);

            //Checking whether there is an else branch
            if(statement.elseBranch != null)
            {
                // Type-check the else branch of the if statement
                statement.elseBranch.Accept(this);
            }

            throw new NotImplementedException();
        }

        public Type VisitInstanceExpression(Expression.InstanceExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitListExpression(Expression.ListExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitLiteralExpression(Expression.LiteralExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitLogicalExpression(Expression.LogicalExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitParameters(OtherNonTerminals.Parameters parameters)
        {
            throw new NotImplementedException();
        }

        public Type VisitPredicateAndExpression(GraphExpression.PredicateAndExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitPredicateGroupingExpression(GraphExpression.PredicateGroupingExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitPredicateLiteralExpression(GraphExpression.PredicateLiteralExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitPredicateOrExpression(GraphExpression.PredicateOrExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitPredicateUnaryExpression(GraphExpression.PredicateUnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitReturnStatement(Statement.ReturnStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitSetExpression(Expression.SetExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitSetFieldExpression(Expression.SetFieldExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitSuperExpression(Expression.SuperExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitThisExpression(Expression.ThisExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitType(OtherNonTerminals.Type type)
        {
            throw new NotImplementedException();
        }

        public Type VisitUnaryExpression(Expression.UnaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitVariableDeclarationStatement(Statement.VariableDeclarationStatement statement)
        {
            var type = statement.type.Accept(this);
            var identifier = statement.type.Accept(this);
            var initializing = statement.initializingExpression.Accept(this);

            if(identifier.type.Value.type != TokenType.IDENTIFIER)
            {
                throw new CheckException("");
            }

            if(statement.initializingExpression != null)
            {
                //Do a type checking whether the initialization matches the declared type
                if(type != initializing)
                {
                    throw new CheckException("");                
                }
            }

            if(variableTable.IsVariableDeclared(statement.identifier.lexeme))
            {
                throw new CheckException("");
            }

            variableTable.AddVariable(statement.identifier.lexeme, statement.type.Accept(this));


            throw new NotImplementedException();
        }

        public Type VisitVariableExpression(Expression.VariableExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitWhileStatement(Statement.WhileStatement statement)
        {
            throw new NotImplementedException();
        }
    }
}
