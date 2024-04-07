using Graphite.Lexer;
using Graphite.Parser;

namespace Graphite.Checkers
{
    internal class TypeChecker :
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
            var leftType = expression.left.Accept(this).type?.type;
            var rightType = expression.right.Accept(this).type?.type;
            if (leftType == null || rightType == null)
            {
                // If 'type' token is null here, it means its a type of Set<> or List<> and we do not
                // allow binary operations on these. TODO should we?
                throw new CheckException("Attempted to do binary operation on non-eligible type");
            }
            leftType = leftType!.Value;
            rightType = rightType!.Value;
            var @operatorType = expression.@operator.type;

            TokenType tokenType;

            switch (leftType)
            {
                case TokenType.STR:
                    tokenType = CheckStringBinaryOperation(operatorType, rightType);
                    break;
                case TokenType.CHAR:
                    tokenType = CheckCharBinaryOperation(operatorType, rightType);
                    break;
                case TokenType.INT:
                    tokenType = CheckIntegerBinaryOperation(operatorType, rightType);
                    break;
                case TokenType.DEC:
                    tokenType = CheckDecimalBinaryOperation(operatorType, rightType);
                    break;
                case TokenType.BOOL:
                    tokenType = CheckBoolBinaryOperation(operatorType, rightType);
                    break;
                default:
                    throw new CheckException("Trying to perform binary operation on non-eligible type");
            }


            var resultToken = new Token();
            resultToken.type = tokenType;

            return new OtherNonTerminals.Type(resultToken, null);
        }

        private TokenType CheckBoolBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            } else if (otherType != TokenType.BOOL)
            {
                throw new BinaryOperationTypeException(TokenType.STR, @operator, otherType);
            }

            List<TokenType> allowedOperators = new List<TokenType>()
            // TODO should we have XOR?
            {TokenType.AND, TokenType.OR, TokenType.EQUAL_EQUAL, TokenType.SLASHED_EQUAL};

            if (allowedOperators.Contains(@operator))
            {
                return TokenType.BOOL;
            } else
            {
                throw new BinaryOperationTypeException(TokenType.STR, @operator, otherType);
            }
        }

        private TokenType CheckDecimalBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            } else if (otherType != TokenType.INT && otherType != TokenType.DEC)
            {
                throw new BinaryOperationTypeException(TokenType.DEC, @operator, otherType);
            }

            List<TokenType> decimalOperators = new List<TokenType>()
            { TokenType.PLUS, TokenType.MINUS, TokenType.SLASH, TokenType.STAR, TokenType.MOD, };
            List<TokenType> boolOperators = new List<TokenType>()
            { TokenType.EQUAL_EQUAL, TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS,
              TokenType.LESS_EQUAL, TokenType.BANG_EQUAL};

            if (decimalOperators.Contains(@operator))
            {
                return TokenType.DEC;
            } else if (boolOperators.Contains(@operator))
            {
                return TokenType.BOOL;
            }
            else
            {
                throw new BinaryOperationTypeException(TokenType.DEC, @operator, otherType);
            }
        }

        private TokenType CheckIntegerBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            }
            else if (otherType != TokenType.INT && otherType != TokenType.DEC)
            {
                throw new BinaryOperationTypeException(TokenType.INT, @operator, otherType);
            }

            List<TokenType> numberOperators = new List<TokenType>()
            { TokenType.PLUS, TokenType.MINUS, TokenType.SLASH, TokenType.STAR, TokenType.MOD, };
            List<TokenType> boolOperators = new List<TokenType>()
            { TokenType.EQUAL_EQUAL, TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS,
              TokenType.LESS_EQUAL, TokenType.BANG_EQUAL};

            if (numberOperators.Contains(@operator))
            {
                return otherType; // dec or int
            }
            else if (boolOperators.Contains(@operator))
            {
                return TokenType.BOOL;
            }
            else
            {
                throw new BinaryOperationTypeException(TokenType.DEC, @operator, otherType);
            }
        }

        private TokenType CheckCharBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            } else if (@operator == TokenType.EQUAL_EQUAL && otherType == TokenType.CHAR)
            {
                return TokenType.BOOL;
            }
            else
            {
                throw new BinaryOperationTypeException(TokenType.CHAR, @operator, otherType);
            }
        }

        private TokenType CheckStringBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.EQUAL_EQUAL && otherType == TokenType.STR)
            {
                return TokenType.BOOL;
            } else if (@operator == TokenType.PLUS)
            {
                return TokenType.STR;
            }
            else
            {
                throw new BinaryOperationTypeException(TokenType.STR, @operator, otherType);
            }
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
