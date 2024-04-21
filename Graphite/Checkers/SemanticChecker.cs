using Graphite.Lexer;
using Graphite.Parser;
using Type = Graphite.Parser.OtherNonTerminals.Type;

namespace Graphite.Checkers
{
    internal class SemanticChecker :
        Statement.IStatementVisitor<Type>,
        Expression.IExpressionVisitor<Type>,
        OtherNonTerminals.IOtherNonTerminalsVisitor<Type>,
        GraphExpression.IGraphExpressionVisitor<Type>
    {
        private readonly VariableTable variableTable = new();
        private readonly FunctionTable functionTable = new();
        private readonly TypeTable typeTable = new();
        private readonly Stack<Type> currentObjectType = new();
        private readonly Stack<string> inFunction = new();

        private bool firstPass;
        private bool isFunctionBlock;

        public void Check(List<Statement> statements)
        {
            try
            {
                BlockStatementImplementation(statements);
            }
            catch (CheckException e)
            {
                Console.WriteLine(e);
            }
        }

        public Type VisitAnonFunctionExpression(Expression.AnonFunctionExpression expression)
        {
            // Enter a new scope for the anonymous function
            variableTable.EnterScope();
            functionTable.EnterScope();
            isFunctionBlock = true;

            // Define the function signature (parameters and return type)
            var parameters = expression.parameters.parameters;

            // Add parameters to the symbol table
            foreach (var parameter in parameters)
            {
                if (!variableTable.IsVariableDeclared(parameter.Item2.lexeme, true))
                {
                    variableTable.AddVariable(parameter.Item2.lexeme, parameter.Item1);
                }
                else
                {
                    throw new CheckException("");
                }
            }

            // Type-check and scope-check the function body
            var returnType = expression.body.Accept(this);

            // Exit the scope after checking the function bodyS
            variableTable.ExitScope();
            functionTable.ExitScope();

            // Define the function type
            var funcTypeArguments = new List<Type> { returnType };
            funcTypeArguments.AddRange(parameters.Select(parameter => parameter.Item1));
            var funcType = new Type(new Token { type = TokenType.FUNC_TYPE }, funcTypeArguments);

            return funcType;
        }

        public Type VisitAssignmentExpression(Expression.AssignmentExpression expression)
        {
            if (!variableTable.IsVariableDeclared(expression.name.lexeme))
            {
                throw new CheckException("Variable has not been declared. Name: " + expression.name.lexeme +
                                         " At line: " + expression.name.line);
            }

            var valueType = expression.value.Accept(this);
            var variableType = variableTable.GetVariableType(expression.name.lexeme);

            if (!CompareTypes(variableType, valueType))
            {
                throw new CheckException(
                    $"Cannot convert {valueType} to {variableType}. At line: {expression.name.line}");
            }

            return null!;
        }

        public Type VisitBinaryExpression(Expression.BinaryExpression expression)
        {
            var leftType = expression.left.Accept(this).type.type;
            var rightType = expression.right.Accept(this).type.type;
            var operatorType = expression.@operator.type;

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


            var resultToken = new Token
            {
                type = tokenType
            };

            return new Type(resultToken, null);
        }

        public Type VisitBlockStatement(Statement.BlockStatement statement)
        {
            return BlockStatementImplementation(statement.statements);
        }

        private Type BlockStatementImplementation(List<Statement> statements)
        {
            var localIsFunctionBlock = isFunctionBlock;
            isFunctionBlock = false;
            if (!localIsFunctionBlock)
            {
                variableTable.EnterScope();
                functionTable.EnterScope();
            }

            firstPass = true;

            foreach (var classDecl in statements.OfType<Statement.ClassDeclarationStatement>())
            {
                if (inFunction.Count > 0) throw new CheckException("Cannot declare a class inside a function.");
                if (typeTable.IsTypeDeclared(classDecl.identifier.lexeme))
                    throw new CheckException("Class already declared. Name: " + classDecl.identifier.lexeme +
                                             " At line: " + classDecl.identifier.line);
                typeTable.AddType(classDecl.identifier.lexeme, classDecl.Accept(this));
            }

            foreach (var functionDecl in statements.OfType<Statement.FunctionDeclarationStatement>())
            {
                if (functionTable.IsFunctionDeclared(functionDecl.identifier.lexeme) ||
                    variableTable.IsVariableDeclared(functionDecl.identifier.lexeme))
                    throw new CheckException("Function already declared. Name: " + functionDecl.identifier.lexeme +
                                             " At line: " + functionDecl.identifier.line);
                functionTable.AddFunction(functionDecl.identifier.lexeme, functionDecl.Accept(this));
            }

            firstPass = false;

            foreach (var singleStatement in statements)
            {
                var result = singleStatement.Accept(this);
                if (singleStatement is not Statement.ReturnStatement) continue;
                if (!localIsFunctionBlock)
                {
                    variableTable.ExitScope();
                    functionTable.ExitScope();
                }

                return result;
            }

            if (!localIsFunctionBlock)
            {
                variableTable.ExitScope();
                functionTable.ExitScope();
            }

            return null!;
        }

        public Type VisitBreakStatement(Statement.BreakStatement statement)
        {
            return null!;
        }

        public Type VisitCallExpression(Expression.CallExpression expression)
        {
            //Callee should return a function type
            var functionType = expression.callee.Accept(this);
            var argumentTypes = expression.arguments.Select(argument => argument.Accept(this)).ToList();

            if (functionType.type.type != TokenType.FUNC_TYPE)
            {
                throw new CheckException("Cannot call a non-function.");
            }

            if (functionType.typeArguments!.Count - 1 != argumentTypes.Count)
            {
                throw new CheckException("Number of arguments does not match the number of parameters.");
            }

            for (var i = 0; i < argumentTypes.Count; i++)
            {
                if (!CompareTypes(functionType.typeArguments[i + 1], argumentTypes[i]))
                {
                    throw new CheckException("Argument type does not match parameter type. Expected: " +
                                             functionType.typeArguments[i + 1].type.type + " Got: " +
                                             argumentTypes[i].type.type);
                }
            }

            return functionType.typeArguments[0].Accept(this);
        }

        public Type VisitClassDeclarationStatement(Statement.ClassDeclarationStatement statement)
        {
            if (firstPass)
            {
                var type = new Type(statement.identifier, null);

                if (statement.extendsIdentifier != null)
                {
                    type.SetSuperClass(statement.extendsIdentifier.Value);
                }

                foreach (var (accessModifier, variableDeclaration) in statement.variableDeclarationStatements)
                {
                    if (accessModifier.type == TokenType.PRIVATE) continue;

                    if (type.HasMember(variableDeclaration.identifier.lexeme))
                    {
                        throw new CheckException("Field or Method already declared. Name: " +
                                                 variableDeclaration.identifier.lexeme + " At line: " +
                                                 statement.identifier.line);
                    }

                    type.AddField((variableDeclaration.identifier.lexeme, variableDeclaration.type.Accept(this)));
                }

                foreach (var (accessModifier, functionDeclaration) in statement.functionDeclarationStatements)
                {
                    if (accessModifier.type == TokenType.PRIVATE) continue;

                    if (type.HasMember(functionDeclaration.identifier.lexeme))
                    {
                        throw new CheckException("Field or Method already declared. Name: " +
                                                 functionDeclaration.identifier.lexeme + " At line: " +
                                                 statement.identifier.line);
                    }

                    type.AddMethod((functionDeclaration.identifier.lexeme, functionDeclaration.Accept(this)));
                }

                return type;
            }

            if (statement.extendsIdentifier != null)
            {
                if (!typeTable.IsTypeDeclared(statement.extendsIdentifier.Value.lexeme))
                {
                    throw new CheckException("Super class not declared. Name: " +
                                             statement.extendsIdentifier.Value.lexeme + " At line: " +
                                             statement.extendsIdentifier.Value.line);
                }
            }

            variableTable.EnterScope();
            functionTable.EnterScope();
            currentObjectType.Push(typeTable.GetType(statement.identifier.lexeme));

            foreach (var (_, variableDeclaration) in statement.variableDeclarationStatements)
            {
                variableDeclaration.Accept(this);
            }

            foreach (var (_, functionDeclaration) in statement.functionDeclarationStatements)
            {
                functionDeclaration.Accept(this);
            }

            currentObjectType.Pop();
            variableTable.ExitScope();
            functionTable.ExitScope();

            return null!;
        }

        public Type VisitContinueStatement(Statement.ContinueStatement statement)
        {
            return null!;
        }

        public Type VisitElementAccessExpression(Expression.ElementAccessExpression expression)
        {
            var collectionType = expression.obj.Accept(this);
            var indexType = expression.index.Accept(this);

            if (indexType.type.type != TokenType.INT)
            {
                throw new CheckException("Index must be of type int.");
            }

            if (collectionType.type.type != TokenType.SET && collectionType.type.type != TokenType.LIST)
            {
                throw new CheckException("Element access can only be used on a set or list.");
            }

            return collectionType.typeArguments![0].Accept(this);
        }

        public Type VisitExpressionStatement(Statement.ExpressionStatement statement)
        {
            statement.expression.Accept(this);
            return null!;
        }

        public Type VisitFunctionDeclarationStatement(Statement.FunctionDeclarationStatement statement)
        {
            inFunction.Push(statement.identifier.lexeme);
            var expectedReturnType = statement.returnType.Accept(this);
            var parameters = statement.parameters.Accept(this);

            if (firstPass)
            {
                var parameterTypes = parameters.typeArguments!.Select(type => type.Accept(this));
                var typeArguments = new List<Type> { expectedReturnType };
                typeArguments.AddRange(parameterTypes);
                var funcType = new Type(new Token { type = TokenType.FUNC_TYPE }, typeArguments);
                inFunction.Pop();
                return funcType;
            }

            variableTable.EnterScope();
            functionTable.EnterScope();
            isFunctionBlock = true;

            foreach (var (parameterType, parameterToken) in statement.parameters.parameters)
            {
                variableTable.AddVariable(parameterToken.lexeme, parameterType.Accept(this));
            }

            var actualReturnType = statement.blockStatement.Accept(this);

            if (!CompareTypes(expectedReturnType, actualReturnType))
            {
                throw new CheckException("Return type does not match the declared return type. Expected: " +
                                         expectedReturnType.type.type + " Got: " + actualReturnType.type.type);
            }

            variableTable.ExitScope();
            functionTable.ExitScope();
            inFunction.Pop();

            return null!;
        }

        public Type VisitGetFieldExpression(Expression.GetFieldExpression expression)
        {
            var objectType = expression.obj.Accept(this);
            currentObjectType.Push(objectType);
            var fieldType = expression.field.Accept(this);

            if (!typeTable.IsTypeDeclared(objectType.type.lexeme))
            {
                throw new CheckException("Object is not of class type.");
            }

            currentObjectType.Pop();

            return fieldType;
        }

        public Type VisitGraphAddVertexExpression(GraphExpression.GraphAddVertexExpression expression)
        {
            var tags = expression.tags.Accept(this).type;
            if (tags.type != TokenType.SET)
            {
                throw new CheckException("Tags argument must be of type set.");
            }

            var times = expression.times.Accept(this).type;
            if (times.type != TokenType.INT)
            {
                throw new CheckException("Times argument must be of type int.");
            }

            return null!;
        }

        public Type VisitGraphBlockStatement(GraphExpression.GraphBlockStatement expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphEdgeExpression(GraphExpression.GraphEdgeExpression expression)
        {
            expression.leftPredicate.Accept(this);
            expression.rightPredicate.Accept(this);
            var weight = expression.weight.Accept(this);

            if (weight.type.type != TokenType.INT && weight.type.type != TokenType.DEC)
            {
                throw new CheckException("Weight must be of type int or dec.");
            }

            return null!;
        }

        public Type VisitGraphExpressionStmt(GraphExpression.GraphExpressionStatement expression)
        {
            expression.statement.Accept(this);
            return null!;
        }

        public Type VisitGraphIfStmt(GraphExpression.GraphIfStatement expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitGraphRemoveVertexExpression(GraphExpression.GraphRemoveVertexExpression expression)
        {
            expression.predicate.Accept(this);
            return null!;
        }

        public Type VisitGraphReTagExpression(GraphExpression.GraphReTagExpression expression)
        {
            var oldTag = expression.oldTag.type;
            var newTag = expression.newTag.type;

            if (oldTag != TokenType.STRING_LITERAL || (newTag != TokenType.STRING_LITERAL && newTag != TokenType.NULL))
            {
                throw new CheckException("Tags must be of type string.");
            }

            return null!;
        }

        public Type VisitGraphStatement(Statement.GraphStatement statement)
        {
            var identifier = statement.identifier;

            if (!variableTable.IsVariableDeclared(identifier.lexeme))
            {
                throw new CheckException("Variable has not been declared. Name: " + identifier.lexeme +
                                                            " At line: " + identifier.line);
            }


            // TODO check if the variable type is a graph type
            //var variableType = variableTable.GetVariableType(identifier.lexeme);
            //if (variableType.type.type)

            statement.expressions.ForEach(expression => expression.Accept(this));

            return null!;
        }

        public Type VisitGraphTagExpression(GraphExpression.GraphTagExpression expression)
        {
            var tags = expression.tags.Accept(this).type;

            if (tags.type != TokenType.SET)
            {
                throw new CheckException("Tags argument must be of type set.");
            }

            expression.predicate.Accept(this);

            return null!;
        }

        public Type VisitGraphWhileStmt(GraphExpression.GraphWhileStatement expression)
        {
            var condition = expression.condition.Accept(this).type;

            if (condition.type != TokenType.BOOL)
            {
                throw new CheckException("Condition must be of type boolean.");
            }

            expression.body.Accept(this);

            return null!;
        }

        public Type VisitGroupingExpression(Expression.GroupingExpression expression)
        {
            return expression.expression.Accept(this);
        }

        public Type VisitIfStatement(Statement.IfStatement statement)
        {
            // Check if the condition expression is a boolean expression
            var conditionType = statement.condition.Accept(this).type.type;
            if (conditionType != TokenType.BOOL)
            {
                throw new CheckException("Condition expression in if statement must be of type boolean.");
            }

            // Type-check the then branch of the if statement
            statement.thenBranch.Accept(this);

            //Type-check the else branch of the if statement
            statement.elseBranch?.Accept(this);

            return null!;
        }

        public Type VisitInstanceExpression(Expression.InstanceExpression expression)
        {
            if (!typeTable.IsTypeDeclared(expression.className.lexeme))
            {
                throw new CheckException("Type has not been declared. Name: " + expression.className.lexeme +
                                         " At line: " + expression.className.line);
            }

            return typeTable.GetType(expression.className.lexeme);
        }

        public Type VisitListExpression(Expression.ListExpression expression)
        {
            var types = expression.elements.Select(element => element.Accept(this)).ToList();

            var firstType = types[0];
            if (types.Any(type => type.type.type != firstType.type.type))
            {
                throw new CheckException("List elements must be of the same type.");
            }

            return new Type(new Token
            {
                type = TokenType.LIST
            }, [firstType]);
        }

        public Type VisitLiteralExpression(Expression.LiteralExpression expression)
        {
            var tokenType = expression.token.type switch
            {
                TokenType.INT_LITERAL => TokenType.INT,
                TokenType.DECIMAL_LITERAL => TokenType.DEC,
                TokenType.CHAR_LITERAL => TokenType.CHAR,
                TokenType.STRING_LITERAL => TokenType.STR,
                TokenType.TRUE => TokenType.BOOL,
                TokenType.FALSE => TokenType.BOOL,
                TokenType.NULL => TokenType.NULL,
                _ => throw new CheckException("Invalid literal type. At line: " + expression.token.line)
            };

            return new Type(new Token
            {
                type = tokenType
            }, null);
        }

        public Type VisitLogicalExpression(Expression.LogicalExpression expression)
        {
            var leftType = expression.left.Accept(this).type.type;
            var rightType = expression.right.Accept(this).type.type;

            if (leftType != TokenType.BOOL || rightType != TokenType.BOOL)
            {
                throw new CheckException("Logical operators can only be used on boolean types. At line: " +
                                         expression.@operator.line);
            }

            return new Type(new Token
            {
                type = TokenType.BOOL
            }, null);
        }

        public Type VisitParameters(OtherNonTerminals.Parameters parameters)
        {
            var types = parameters.parameters.Select(parameter => parameter.Item1.Accept(this)).ToList();
            return new Type(new Token(), types);
        }

        public Type VisitPredicateAndExpression(GraphExpression.PredicateAndExpression expression)
        {
            var leftType = expression.left.Accept(this).type.type;
            var rightType = expression.right.Accept(this).type.type;

            if (leftType == TokenType.BOOL && rightType == TokenType.BOOL)
            {
                return new Type(new Token { type = TokenType.BOOL }, null);
            }
            else
            {
                throw new CheckException("Both sides of the AND expression must be of type boolean.");
            }
        }

        public Type VisitPredicateGroupingExpression(GraphExpression.PredicateGroupingExpression expression)
        {
            return expression.expression.Accept(this);
        }

        public Type VisitPredicateLiteralExpression(GraphExpression.PredicateLiteralExpression expression)
        {
            var tokenType = expression.expression.Accept(this).type.type;

            var predicateTypes = new List<TokenType>
            {
                TokenType.STR, TokenType.STRING_LITERAL
            };

            if (!predicateTypes.Contains(tokenType))
            {
                throw new CheckException("Predicate must be of type string.");
            }

            return new Type(new Token { type = TokenType.BOOL }, null);
        }

        public Type VisitPredicateOrExpression(GraphExpression.PredicateOrExpression expression)
        {
            var leftType = expression.left.Accept(this).type.type;
            var rightType = expression.right.Accept(this).type.type;

            if (leftType != TokenType.BOOL || rightType != TokenType.BOOL)
            {
                throw new CheckException("Both sides of the OR expression must be of type boolean.");
            }

            return new Type(new Token { type = TokenType.BOOL }, null);
        }

        public Type VisitPredicateUnaryExpression(GraphExpression.PredicateUnaryExpression expression)
        {
            var rightType = expression.right.Accept(this).type.type;
            var @operatorType = expression.@operator.type;

            switch (operatorType)
            {
                case TokenType.MINUS:
                    if (rightType == TokenType.INT_LITERAL)
                    {
                        return new Type(new Token { type = TokenType.INT }, null);
                    }
                    else if (rightType != TokenType.DECIMAL_LITERAL)
                    {
                        return new Type(new Token { type = TokenType.DEC }, null);
                    }
                    else
                    {
                        throw new CheckException("The right hand side of the minus must be of type int or decimal.");
                    }
                case TokenType.BANG:
                    if (rightType == TokenType.BOOL)
                    {
                        return new Type(new Token { type = TokenType.BOOL }, null);
                    }
                    else
                    {
                        throw new CheckException("The right side of the NOT expression must be of type boolean.");
                    }
                default:
                    throw new CheckException("Trying to perform unary operation on non-eligible type");
            }
        }

        public Type VisitReturnStatement(Statement.ReturnStatement statement)
        {
            return statement.expression.Accept(this);
        }

        public Type VisitSetExpression(Expression.SetExpression expression)
        {
            var types = expression.elements.Select(element => element.Accept(this)).ToList();

            var firstType = types[0];
            if (types.Any(type => !CompareTypes(type, firstType)))
            {
                throw new CheckException("Set elements must be of the same type.");
            }

            return new Type(new Token
            {
                type = TokenType.SET
            }, [firstType]);
        }

        public Type VisitSetFieldExpression(Expression.SetFieldExpression expression)
        {
            var objectType = expression.obj.Accept(this);
            currentObjectType.Push(objectType);
            var fieldType = expression.field.Accept(this);

            if (!typeTable.IsTypeDeclared(objectType.type.lexeme))
            {
                throw new CheckException("Object is not of class type.");
            }

            currentObjectType.Pop();

            var valueType = expression.value.Accept(this);

            if (!CompareTypes(fieldType, valueType))
            {
                throw new CheckException("Type mismatch.");
            }

            return fieldType;
        }

        public Type VisitSuperExpression(Expression.SuperExpression expression)
        {
            if (currentObjectType.Count == 0)
            {
                throw new CheckException("Cannot use super outside of a class.");
            }

            if (currentObjectType.Peek().SuperClass == null)
            {
                throw new CheckException("Class does not have a super class.");
            }

            return typeTable.GetType(currentObjectType.Peek().SuperClass!.Value.lexeme);
        }

        public Type VisitThisExpression(Expression.ThisExpression expression)
        {
            if (currentObjectType.Count == 0)
            {
                throw new CheckException("Cannot use this outside of a class.");
            }

            return currentObjectType.Peek();
        }

        public Type VisitType(Type type)
        {
            if (type.type.type != TokenType.IDENTIFIER) return type;
            if (typeTable.IsTypeDeclared(type.type.lexeme)) return typeTable.GetType(type.type.lexeme);
            if (firstPass)
            {
                var dummyType = new Type(type.type, null, true);
                typeTable.AddType(type.type.lexeme, dummyType);
                return type;
            }

            throw new CheckException("Type has not been declared. Name: " + type.type.lexeme +
                                     " At line: " + type.type.line);
        }

        public Type VisitUnaryExpression(Expression.UnaryExpression expression)
        {
            var rightType = expression.right.Accept(this).type.type;
            var @operatorType = expression.@operator.type;

            var resultToken = new Token();

            if (operatorType == TokenType.MINUS)
            {
                if (rightType == TokenType.INT | rightType == TokenType.DEC)
                {
                    resultToken.type = rightType;
                }
                else throw new CheckException("");
            }
            else if (operatorType == TokenType.BANG)
            {
                if (rightType == TokenType.BOOL)
                {
                    resultToken.type = rightType;
                }
                else throw new CheckException("");
            }

            return new Type(resultToken, null);
        }

        public Type VisitVariableDeclarationStatement(Statement.VariableDeclarationStatement statement)
        {
            var type = statement.type.Accept(this);
            var initializing = statement.initializingExpression?.Accept(this);

            if (initializing != null)
            {
                //Do a type checking whether the initialization matches the declared type
                if (!CompareTypes(type, initializing))
                {
                    throw new CheckException("Type mismatch. At line: " + statement.identifier.line);
                }
            }

            if (currentObjectType.Count > 0)
            {
                return null!;
            }

            if (variableTable.IsVariableDeclared(statement.identifier.lexeme, inFunction.Count > 0) ||
                functionTable.IsFunctionDeclared(statement.identifier.lexeme, inFunction.Count > 0))
            {
                throw new CheckException("Member with same name already declared. Name: " +
                                         statement.identifier.lexeme +
                                         " At line: " + statement.identifier.line);
            }

            variableTable.AddVariable(statement.identifier.lexeme, statement.type.Accept(this));

            return null!;
        }

        public Type VisitVariableExpression(Expression.VariableExpression expression)
        {
            if (currentObjectType.Count > 0)
            {
                if (currentObjectType.Peek().fields.TryGetValue(expression.name.lexeme, out var variableType))
                {
                    return variableType.type.type == TokenType.IDENTIFIER
                        ? typeTable.GetType(variableType.type.lexeme)
                        : variableType;
                }

                if (currentObjectType.Peek().methods.TryGetValue(expression.name.lexeme, out var functionType))
                {
                    return functionType;
                }

                if (currentObjectType.Peek().SuperClass == null)
                {
                    throw new CheckException("Field or method does not exist. Name: " + expression.name.lexeme +
                                             " At line: " + expression.name.line);
                }

                currentObjectType.Push(typeTable.GetType(currentObjectType.Peek().SuperClass!.Value.lexeme));
                var result = expression.Accept(this);
                currentObjectType.Pop();
                return result;
            }

            if (variableTable.IsVariableDeclared(expression.name.lexeme))
            {
                return variableTable.GetVariableType(expression.name.lexeme);
            }

            if (functionTable.IsFunctionDeclared(expression.name.lexeme))
            {
                return functionTable.GetFunctionType(expression.name.lexeme);
            }

            throw new CheckException("Variable has not been declared. Name: " + expression.name.lexeme + " At line: " +
                                     expression.name.line);
        }

        public Type VisitWhileStatement(Statement.WhileStatement statement)
        {
            if (statement.condition.Accept(this).type.type != TokenType.BOOL)
            {
                throw new CheckException("Condition expression in if statement must be of type boolean.");
            }

            statement.body.Accept(this);

            return null!;
        }

        private static bool CompareTypes(Type type1, Type type2)
        {
            var type1ArgLength = type1.typeArguments?.Count ?? 0;
            var type2ArgLength = type2.typeArguments?.Count ?? 0;

            if (type1ArgLength != type2ArgLength) return false;

            for (var i = 0; i < type1ArgLength; i++)
            {
                if (!CompareTypes(type1.typeArguments![i], type2.typeArguments![i]))
                {
                    return false;
                }
            }

            if (type1.type.type == TokenType.IDENTIFIER && type2.type.type == TokenType.IDENTIFIER)
            {
                return type1.type.lexeme == type2.type.lexeme;
            }

            return type1.type.type == type2.type.type;
        }

        private static TokenType CheckBoolBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            }

            if (otherType != TokenType.BOOL)
            {
                throw new BinaryOperationTypeException(TokenType.STR, @operator, otherType);
            }

            List<TokenType> allowedOperators =
                [TokenType.AND, TokenType.OR, TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL];

            if (allowedOperators.Contains(@operator))
            {
                return TokenType.BOOL;
            }

            throw new BinaryOperationTypeException(TokenType.STR, @operator, otherType);
        }

        private static TokenType CheckDecimalBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            }

            if (otherType != TokenType.INT && otherType != TokenType.DEC)
            {
                throw new BinaryOperationTypeException(TokenType.DEC, @operator, otherType);
            }

            List<TokenType> decimalOperators = new List<TokenType>
                { TokenType.PLUS, TokenType.MINUS, TokenType.SLASH, TokenType.STAR, TokenType.MOD, };
            List<TokenType> boolOperators = new List<TokenType>
            {
                TokenType.EQUAL_EQUAL, TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS,
                TokenType.LESS_EQUAL, TokenType.BANG_EQUAL
            };

            if (decimalOperators.Contains(@operator))
            {
                return TokenType.DEC;
            }

            if (boolOperators.Contains(@operator))
            {
                return TokenType.BOOL;
            }

            throw new BinaryOperationTypeException(TokenType.DEC, @operator, otherType);
        }

        private static TokenType CheckIntegerBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            }

            if (otherType != TokenType.INT && otherType != TokenType.DEC)
            {
                throw new BinaryOperationTypeException(TokenType.INT, @operator, otherType);
            }

            List<TokenType> numberOperators = new List<TokenType>
                { TokenType.PLUS, TokenType.MINUS, TokenType.SLASH, TokenType.STAR, TokenType.MOD, };
            List<TokenType> boolOperators = new List<TokenType>
            {
                TokenType.EQUAL_EQUAL, TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS,
                TokenType.LESS_EQUAL, TokenType.BANG_EQUAL
            };

            if (numberOperators.Contains(@operator))
            {
                return otherType; // dec or int
            }

            if (boolOperators.Contains(@operator))
            {
                return TokenType.BOOL;
            }

            throw new BinaryOperationTypeException(TokenType.INT, @operator, otherType);
        }

        private static TokenType CheckCharBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.PLUS && otherType == TokenType.STR)
            {
                return TokenType.STR;
            }

            if (@operator == TokenType.EQUAL_EQUAL && otherType == TokenType.CHAR)
            {
                return TokenType.BOOL;
            }

            throw new BinaryOperationTypeException(TokenType.CHAR, @operator, otherType);
        }

        private static TokenType CheckStringBinaryOperation(TokenType @operator, TokenType otherType)
        {
            if (@operator == TokenType.EQUAL_EQUAL && otherType == TokenType.STR)
            {
                return TokenType.BOOL;
            }

            if (@operator == TokenType.PLUS)
            {
                return TokenType.STR;
            }

            throw new BinaryOperationTypeException(TokenType.STR, @operator, otherType);
        }
    }
}