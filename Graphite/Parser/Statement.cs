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
        public readonly Expression expression;
        
        public ExpressionStatement (Expression expression)
        {
            this.expression = expression;
        }
        
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
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclarationStatement(this);
        }
    }
}