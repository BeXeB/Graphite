using Graphite.Lexer;
using Graphite.Parser;

namespace Graphite;

public abstract class Statement
{
    public interface IStatementVisitor<T>
    {
        T VisitBlockStatement (BlockStatement statement);
        T VisitExpressionStatement (ExpressionStatement statement);
        T VisitIfStatement (IfStatement statement);
        T VisitWhileStatement (WhileStatement statement);
        T VisitReturnStatement (ReturnStatement statement);
        T VisitBreakStatement (BreakStatement statement);
        T VisitContinueStatement (ContinueStatement statement);
        T VisitGraphStatement (GraphStatement statement);
        T VisitClassDeclarationStatement (ClassDeclarationStatement statement);
        T VisitFunctionDeclarationStatement (FunctionDeclarationStatement statement);
        T VisitVariableDeclarationStatement (VariableDeclarationStatement statement);
    }
    public abstract T Accept<T> (IStatementVisitor<T> visitor);
    
    public class BlockStatement : Statement
    {
        public readonly List<Statement> statements;
        
        public BlockStatement (List<Statement> statements)
        {
            this.statements = statements;
        }
        
        public override T Accept<T> (IStatementVisitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        }
    }
    
    public class ExpressionStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
    public class IfStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }
    public class WhileStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }
    }
    public class ReturnStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }
    }
    public class BreakStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }
    }
    public class ContinueStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitContinueStatement(this);
        }
    }
    public class GraphStatement : Statement
    {
        public readonly Token identifier;
        public readonly List<GraphExpression> expressions;
        
        public GraphStatement (Token identifier ,List<GraphExpression> expressions)
        {
            this.expressions = expressions;
            this.identifier = identifier;
        }
        
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitGraphStatement(this);
        }
    }
    public class ClassDeclarationStatement : Statement
    {
        public readonly Token accessModifier;
        public readonly Token identifier;
        public readonly Token? extends;
        public readonly Token? extendsIdentifier;

        public readonly List<VariableDeclarationStatement> variableDeclarationStatements;
        public readonly List <FunctionDeclarationStatement> functionDeclarationStatements;

        public ClassDeclarationStatement(
            Token accessModifier, 
            Token identifier, 
            Token? extends, 
            Token? extendsIdentifier, 
            List<VariableDeclarationStatement> variableDeclarationStatements, 
            List<FunctionDeclarationStatement> functionDeclarationStatements)
        {
            this.accessModifier = accessModifier;
            this.identifier = identifier;
            this.extends = extends;
            this.extendsIdentifier = extendsIdentifier;
            this.variableDeclarationStatements = variableDeclarationStatements ?? new List<VariableDeclarationStatement>();
            this.functionDeclarationStatements = functionDeclarationStatements ?? new List<FunctionDeclarationStatement>();
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitClassDeclarationStatement(this);
        }
    }
    public class FunctionDeclarationStatement : Statement
    {
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunctionDeclarationStatement(this);
        }
    }
    public class VariableDeclarationStatement : Statement
    {
        public readonly Token type;
        public readonly Token identifier;
        public readonly Expression? initializingExpression;

        public VariableDeclarationStatement(Token type, Token identifier, Expression? initializingExpression)
        {
            this.type = type;
            this.identifier = identifier;
            this.initializingExpression = initializingExpression;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclarationStatement(this);
        }
    }
}