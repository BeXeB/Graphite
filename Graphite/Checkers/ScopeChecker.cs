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
        private VariableTable variableTable;
        private FunctionTable functionTable;
        private TypeTable typeTable;

        private bool firstPass;

        private Type currentObjectType = null!;
        private bool isInGetField = false;

        public ScopeChecker()
        {
            variableTable = new VariableTable();
            functionTable = new FunctionTable();
            typeTable = new TypeTable();
        }

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
                    throw new CheckException("Class already declared.");
                typeTable.AddType(classDecl.identifier.lexeme, classDecl.Accept(this));
            }

            foreach (var functionDecl in statements.OfType<Statement.FunctionDeclarationStatement>())
            {
                if (functionTable.IsFunctionDeclared(functionDecl.identifier.lexeme))
                    throw new CheckException("Function already declared.");
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

            if (functionType.type.Value.type != TokenType.FUNC_TYPE)
            {
                throw new CheckException("Cannot call a non-function.");
            }

            if (functionType.typeArguments.Count - 1 != argumentTypes.Count)
            {
                throw new CheckException("Number of arguments does not match the number of parameters.");
            }

            for (var i = 0; i < argumentTypes.Count; i++)
            {
                if (functionType.typeArguments[i + 1] != argumentTypes[i])
                {
                    throw new CheckException("Argument type does not match parameter type.");
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

                foreach (var variableDeclaration in statement.variableDeclarationStatements)
                {
                    type.AddField((variableDeclaration.identifier.lexeme, variableDeclaration.type.Accept(this)));
                }

                foreach (var functionDeclaration in statement.functionDeclarationStatements)
                {
                    type.AddMethod((functionDeclaration.identifier.lexeme, functionDeclaration.Accept(this)));
                }

                return type;
            }

            if (statement.extendsIdentifier != null)
            {
                if (!typeTable.IsTypeDeclared(statement.extendsIdentifier.Value.lexeme))
                {
                    throw new CheckException("Super class not declared.");
                }
            }

            variableTable.EnterScope();
            functionTable.EnterScope();

            foreach (var variableDeclaration in statement.variableDeclarationStatements)
            {
                variableDeclaration.Accept(this);
            }

            foreach (var functionDeclaration in statement.functionDeclarationStatements)
            {
                functionDeclaration.Accept(this);
            }

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

            if (indexType.type.Value.type != TokenType.INT)
            {
                throw new CheckException("Index must be of type int.");
            }

            //TODO: check if the collection type is a SET or LIST

            return collectionType.typeArguments[0].Accept(this);
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
                var parameterTypes = parameters.typeArguments.Select(type => type.Accept(this));
                var typeArguments = new List<Type> { returnType };
                typeArguments.AddRange(parameterTypes);
                return new Type(new Token { type = TokenType.FUNC_TYPE }, typeArguments);
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
            isInGetField = true;
            currentObjectType = objectType;
            var fieldType = expression.field.Accept(this);

            if (!typeTable.IsTypeDeclared(objectType.type.Value.lexeme))
            {
                throw new CheckException("Object is not of class type.");
            }
            
            currentObjectType = null!;
            isInGetField = false;
            
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
            TokenType conditionType = statement.condition.Accept(this).type.Value.type;
            if (conditionType != TokenType.BOOL)
            {
                throw new CheckException("Condition expression in if statement must be of type boolean.");
            }

            // Type-check the then branch of the if statement
            statement.thenBranch.Accept(this);

            //Checking whether there is an else branch
            if (statement.elseBranch != null)
            {
                // Type-check the else branch of the if statement
                statement.elseBranch.Accept(this);
            }

            throw new NotImplementedException();
        }

        public Type VisitInstanceExpression(Expression.InstanceExpression expression)
        {
            if (!typeTable.IsTypeDeclared(expression.className.lexeme))
            {
                throw new CheckException("Type has not been declared.");
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
                _ => throw new CheckException("Invalid literal type.")
            };

            return new Type(new Token
            {
                type = tokenType, lexeme = "", literal = null
            }, null);
        }

        public Type VisitLogicalExpression(Expression.LogicalExpression expression)
        {
            throw new NotImplementedException();
        }

        public Type VisitParameters(OtherNonTerminals.Parameters parameters)
        {
            var types = parameters.parameters.Select(parameter => parameter.Item1.Accept(this)).ToList();
            return new Type(new(), types);
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

        public Type VisitType(OtherNonTerminals.Type type)
        {
            if (type.type.Value.type != TokenType.IDENTIFIER) return type;
            if (!typeTable.IsTypeDeclared(type.type.Value.lexeme))
            {
                throw new CheckException("Type has not been declared.");
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
            var identifier = statement.type.Accept(this);
            Type? initializing = null;
            if (statement.initializingExpression != null)
                initializing = statement.initializingExpression.Accept(this);

            if (statement.initializingExpression != null)
            {
                //Do a type checking whether the initialization matches the declared type
                if (!type.Equals(initializing))
                {
                    throw new CheckException("Type mismatch.");
                }
            }

            if (variableTable.IsVariableDeclared(statement.identifier.lexeme))
            {
                throw new CheckException("Variable already declared.");
            }

            variableTable.AddVariable(statement.identifier.lexeme, statement.type.Accept(this));


            return null!;
        }

        public Type VisitVariableExpression(Expression.VariableExpression expression)
        {
            if (isInGetField)
            {
                if (currentObjectType.fields.TryGetValue(expression.name.lexeme, out var variableType))
                {
                    return variableType;
                }

                if (currentObjectType.methods.TryGetValue(expression.name.lexeme, out var functionType))
                {
                    return functionType;
                }
                
                throw new CheckException("Field or method does not exist.");
            }
            
            if (variableTable.IsVariableDeclared(expression.name.lexeme))
            {
                return variableTable.GetVariableType(expression.name.lexeme);
            }

            if (functionTable.IsFunctionDeclared(expression.name.lexeme))
            {
                return functionTable.GetFunctionType(expression.name.lexeme);
            }

            throw new CheckException("Variable has not been declared.");
        }

        public Type VisitWhileStatement(Statement.WhileStatement statement)
        {
            //TO DO: check that the condition is boolean
            //TO DO: for checking the body, call accept method

            throw new NotImplementedException();
        }
    }
}