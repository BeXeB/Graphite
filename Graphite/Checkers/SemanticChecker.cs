using Graphite.Lexer;
using Graphite.Parser;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;

namespace Graphite.Checkers
{
    internal class SemanticChecker :
        Statement.IStatementVisitor<OtherNonTerminals.Type>,
        Graphite.Parser.Expression.IExpressionVisitor<OtherNonTerminals.Type>,
        OtherNonTerminals.IOtherNonTerminalsVisitor<OtherNonTerminals.Type>,
        GraphExpression.IGraphExpressionVisitor<OtherNonTerminals.Type>
    {
        // TODO add global scoope
        // TODO how about "global" scope, inside a class (functions being able to use functions/variables declered below itself
        private Stack<Dictionary<string, Variable>> variableScopes = new Stack<Dictionary<string, Variable>>();
        private Stack<Dictionary<string, Method>> methodScopes = new Stack<Dictionary<string, Method>>();
        // TODO consider if i should merge method and variable scopes into one
        public class Variable
        {
            public string Name;
            public OtherNonTerminals.Type Type;
            public bool IsInitialized;
        }
        
        public class Method
        {
            public string Name;
            public List<Variable> Parameters;
            public OtherNonTerminals.Type ReturnType;
        }

        public class CustomType
        {
            public TypeOfType typeType;
            public string Name;
            public CustomType? baseType;
            public List<Method> publicMethods;
            public List<Variable> publicVariables;
            public enum TypeOfType
            {
                Class,
                Struct
            }
        }

        private void BeginScope()
        {
            variableScopes.Push(new Dictionary<String, Variable>());
            methodScopes.Push(new Dictionary<String, Method>());
        }

        private void EndScope()
        {
            variableScopes.Pop();
            methodScopes.Pop();
        }
        
        // TODO confirm if this works
        // TODO add possibility to find the right overloaded version of the method
        private Method FindClosestMethod(string methodName)
        {
            var result = methodScopes.SelectMany(scope => scope)
                .FirstOrDefault(methodEntry => methodEntry.Key == methodName);

            if (result.Key == null)
            {
                throw new CheckException("Trying to reference non-existing method in scope");
            }

            return result.Value;
        }

        // TODO confirm if this works
        private Variable FindClosestVariable(string variableName)
        {
            var result = variableScopes.SelectMany(scope => scope)
                .FirstOrDefault(variableEntry => variableEntry.Key == variableName);

            if (result.Key == null)
            {
                throw new CheckException("Trying to reference non-existing variable in scope");
            }

            return result.Value;
        }

        public OtherNonTerminals.Type VisitBlockStatement(Statement.BlockStatement statement)
        {
            // NOTE: we cant begin the scope here. For example methods that have parameters, should add a new scope with the parameters before
            // visiting the block statement
            foreach (Statement currStm in statement.statements)
            {
                currStm.Accept(this);
            }
            EndScope();
            return null;
        }


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
            var leftTypeNullable = expression.left.Accept(this).type?.type;
            var rightTypeNullable = expression.right.Accept(this).type?.type;
            if (leftTypeNullable == null || rightTypeNullable == null)
            {
                // If 'type' token is null here, it means its a type of Set<> or List<> and we do not
                // allow binary operations on these. TODO should we?
                throw new CheckException("Attempted to do binary operation on non-eligible type");
            }
            var leftType = leftTypeNullable.Value;
            var rightType = rightTypeNullable.Value;
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
                throw new BinaryOperationTypeException(TokenType.INT, @operator, otherType);
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


        public OtherNonTerminals.Type VisitBreakStatement(Statement.BreakStatement statement)
        {
            EndScope();
            return null;
        }

        public OtherNonTerminals.Type VisitCallExpression(Expression.CallExpression expression)
        {
            var functionType = expression.callee.Accept(this);

            var arguments = expression.arguments.Select(x =>
            {
                return x.Accept(this);
            });

            int i = 1; // because the 1st argument type (0th index) is the return type
            foreach(var currArg in arguments)
            {
                Compare(currArg.type, functionType.typeArguments[i]);
                i++;
            }

            //FindClosestMethod(expression.callee); //TODO finnish this when callexpression has been fixed
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitClassDeclarationStatement(Statement.ClassDeclarationStatement statement)
        {
            // TODO handle accessmodifier making things available outside scope
            // TODO handle extends
            BeginScope();
            foreach(var currStatement in statement.variableDeclarationStatements)
            {
                currStatement.Accept(this);
            }
            foreach(var currStatement in statement.functionDeclarationStatements)
            {
                currStatement.Accept(this);
            }
            EndScope();

            // TODO add to type scope
            throw new NotImplementedException();
        }

        public OtherNonTerminals.Type VisitContinueStatement(Statement.ContinueStatement statement)
        {
            return null; // This does not need to do anything right
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
            // TODO take parameter types into consideration as well to support overloading
            var methodExisting = methodScopes.Peek().TryGetValue(statement.identifier.lexeme, out Method existingMethod);

            if (methodExisting)
            {
                throw new CheckException("Function with same name and signature has already been declared inside scope");
            }

            BeginScope();
            statement.parameters.Accept(this);
            statement.blockStatement.Accept(this);
            EndScope();

            var newMethod = new Method()
            {
                Name = statement.identifier.lexeme,
                Parameters = null, // TODO add this to support overloading
                ReturnType = statement.returnType,
            };

            methodScopes.Peek().Add(statement.identifier.lexeme, newMethod);

            return statement.returnType;
        }

        public OtherNonTerminals.Type VisitGetFieldExpression(Expression.GetFieldExpression expression)
        {
            throw new NotImplementedException(); //TODO make when getfieldexpresison is fixed
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
            var conditionType = statement.condition.Accept(this).type!.Value.lexeme;
                //expression.condition.Accept(this).type!.Value;
            if (conditionType.ToString() != "BOOL") // TODO this does not seem like a clean way to do it
            {
                throw new CheckException("Expression inside if-statement must evaluate to a boolean value");
            }

            BeginScope();
            statement.thenBranch.Accept(this);
            EndScope();

            if(statement.elseBranch != null)
            {
                BeginScope();
                statement.elseBranch.Accept(this);
                EndScope();
            }

            return null;
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

        public bool CompareTypes(OtherNonTerminals.Type type1, OtherNonTerminals.Type type2) {
            var type1ArgLength = type1.typeArguments != null ? type1.typeArguments.Count : 0;
            var type2ArgLength = type2.typeArguments != null ? type2.typeArguments.Count : 0;

            if (type1ArgLength == type2ArgLength)
            {
                for(int i = 0; i < type1ArgLength; i++)
                {
                    if (!CompareTypes(type1.typeArguments![i], type2.typeArguments![i]))
                    {
                        return false;
                    }
                }
                if (type1.type.Value.type == TokenType.IDENTIFIER && type2.type.Value.type == TokenType.IDENTIFIER)
                {
                    return type1.type.Value.type == type2.type.Value.type;
                }
                else
                {
                    return type1.type.Value.lexeme == type2.type.Value.lexeme;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
