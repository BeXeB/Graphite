using Type = Graphite.Parser.OtherNonTerminals.Type;
using Graphite.Parser;
using Graphite.Lexer;

namespace Graphite.Checkers
{
    internal class ScopeChecker :
        Statement.IStatementVisitor<Type>,
        Expression.IExpressionVisitor<Type>,
        OtherNonTerminals.IOtherNonTerminalsVisitor<Type>,
        GraphExpression.IGraphExpressionVisitor<Type>
    {
        private readonly VariableTable variableTable = new();
        private readonly FunctionTable functionTable = new();
        private readonly TypeTable typeTable = new();

        private bool firstPass;

        private readonly Stack<Type> currentObjectType = new();

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
            throw new NotImplementedException();
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

            //TO DO: create a check also for the name

            throw new NotImplementedException();
        }

        public Type VisitBinaryExpression(Expression.BinaryExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitBlockStatement(Statement.BlockStatement statement)
        {
            return BlockStatementImplementation(statement.statements);
        }

        private Type BlockStatementImplementation(List<Statement> statements)
        {
            // Enter a new scope for the body of the if statement
            variableTable.EnterScope();
            functionTable.EnterScope();

            firstPass = true;

            foreach (var classDecl in statements.OfType<Statement.ClassDeclarationStatement>())
            {
                if (typeTable.IsTypeDeclared(classDecl.identifier.lexeme))
                    throw new CheckException("Class already declared. Name: " + classDecl.identifier.lexeme +
                                             " At line: " + classDecl.identifier.line);
                typeTable.AddType(classDecl.identifier.lexeme, classDecl.Accept(this));
            }

            foreach (var functionDecl in statements.OfType<Statement.FunctionDeclarationStatement>())
            {
                if (functionTable.IsFunctionDeclared(functionDecl.identifier.lexeme))
                    throw new CheckException("Function already declared. Name: " + functionDecl.identifier.lexeme +
                                             " At line: " + functionDecl.identifier.line);
                functionTable.AddFunction(functionDecl.identifier.lexeme, functionDecl.Accept(this));
            }

            firstPass = false;

            foreach (var singleStatement in statements)
            {
                singleStatement.Accept(this);
            }

            // Exit the scope after checking the body
            variableTable.ExitScope();
            functionTable.ExitScope();

            return null!;
        }

        public Type VisitBreakStatement(Statement.BreakStatement statement)
        {
            throw new NotImplementedException();
        }

        public Type VisitCallExpression(Expression.CallExpression expression)
        {
            //Callee should return a function type
            var functionType = expression.callee.Accept(this);
            var argumentTypes = expression.arguments.Select(argument => argument.Accept(this)).ToList();

            if (functionType.type!.Value.type != TokenType.FUNC_TYPE)
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
                                             functionType.typeArguments[i + 1].type!.Value.type + " Got: " +
                                             argumentTypes[i].type!.Value.type);
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

                var identifier = "";
                try
                {
                    foreach (var (accessModifier, variableDeclaration) in statement.variableDeclarationStatements)
                    {
                        if (accessModifier.type == TokenType.PRIVATE) continue;
                        identifier = variableDeclaration.identifier.lexeme;
                        type.AddField((variableDeclaration.identifier.lexeme, variableDeclaration.type.Accept(this)));
                    }

                    foreach (var (accessModifier, functionDeclaration) in statement.functionDeclarationStatements)
                    {
                        if (accessModifier.type == TokenType.PRIVATE) continue;
                        identifier = functionDeclaration.identifier.lexeme;
                        type.AddMethod((functionDeclaration.identifier.lexeme, functionDeclaration.Accept(this)));
                    }
                }
                catch (ArgumentException)
                {
                    throw new CheckException("Field or Method already declared. Name: " + identifier + " At line: " +
                                             statement.identifier.line);
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
            throw new NotImplementedException();
        }

        public Type VisitElementAccessExpression(Expression.ElementAccessExpression expression)
        {
            var collectionType = expression.obj.Accept(this);
            var indexType = expression.index.Accept(this);

            if (indexType.type!.Value.type != TokenType.INT)
            {
                throw new CheckException("Index must be of type int.");
            }

            if (collectionType.type!.Value.type != TokenType.SET && collectionType.type!.Value.type != TokenType.LIST)
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
            if (firstPass)
            {
                var returnType = statement.returnType.Accept(this);
                var parameters = statement.parameters.Accept(this);
                var parameterTypes = parameters.typeArguments!.Select(type => type.Accept(this));
                var typeArguments = new List<Type> { returnType };
                typeArguments.AddRange(parameterTypes);
                var funcType = new Type(new Token { type = TokenType.FUNC_TYPE }, typeArguments);
                return funcType;
            }

            variableTable.EnterScope();
            functionTable.EnterScope();

            foreach (var (parameterType, parameterToken) in statement.parameters.parameters)
            {
                variableTable.AddVariable(parameterToken.lexeme, parameterType.Accept(this));
            }

            statement.blockStatement.Accept(this);

            variableTable.ExitScope();
            functionTable.ExitScope();

            return null!;
        }

        public Type VisitGetFieldExpression(Expression.GetFieldExpression expression)
        {
            var objectType = expression.obj.Accept(this);
            currentObjectType.Push(objectType);
            var fieldType = expression.field.Accept(this);

            if (!typeTable.IsTypeDeclared(objectType.type!.Value.lexeme))
            {
                throw new CheckException("Object is not of class type.");
            }

            currentObjectType.Pop();

            return fieldType;
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
            var conditionType = statement.condition.Accept(this).type!.Value.type;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Type VisitParameters(OtherNonTerminals.Parameters parameters)
        {
            var types = parameters.parameters.Select(parameter => parameter.Item1.Accept(this)).ToList();
            return new Type(new Token(), types);
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
            return statement.expression.Accept(this);
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

        public Type VisitType(Type type)
        {
            if (type.type!.Value.type != TokenType.IDENTIFIER) return type;
            if (!typeTable.IsTypeDeclared(type.type.Value.lexeme))
            {
                throw new CheckException("Type has not been declared. Name: " + type.type.Value.lexeme + " At line: " +
                                         type.type.Value.line);
            }

            return typeTable.GetType(type.type.Value.lexeme);
        }

        public Type VisitUnaryExpression(Expression.UnaryExpression expression)
        {
            //TO DO: similarly to binary, go through every possible unary operator and check the possible type usages


            throw new NotImplementedException();
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

            if (variableTable.IsVariableDeclared(statement.identifier.lexeme))
            {
                throw new CheckException("Variable already declared. Name: " + statement.identifier.lexeme +
                                         " At line: " + statement.identifier.line);
            }

            variableTable.AddVariable(statement.identifier.lexeme, statement.type.Accept(this));

            return null!;
        }

        public Type VisitVariableExpression(Expression.VariableExpression expression)
        {
            if (currentObjectType.Count > 0)
            {
                while (true)
                {
                    if (currentObjectType.Peek().fields.TryGetValue(expression.name.lexeme, out var variableType))
                    {
                        return variableType;
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

                    var type = currentObjectType.Pop();
                    currentObjectType.Push(typeTable.GetType(type.SuperClass!.Value.lexeme));
                }
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
            //TO DO: check that the condition is boolean
            //TO DO: for checking the body, call accept method

            throw new NotImplementedException();
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

            if (type1.type!.Value.type == TokenType.IDENTIFIER && type2.type!.Value.type == TokenType.IDENTIFIER)
            {
                return type1.type.Value.lexeme == type2.type.Value.lexeme;
            }

            return type1.type.Value.type == type2.type!.Value.type;
        }
    }
}