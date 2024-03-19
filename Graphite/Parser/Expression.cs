using Graphite.Lexer;

namespace Graphite.Parser;

public abstract class Expression
{
    public interface IExpressionVisitor<T>
    {
        T VisitBinaryExpression (BinaryExpression expression);
        T VisitGroupingExpression (GroupingExpression expression);
        T VisitLiteralExpression (LiteralExpression expression);
        T VisitUnaryExpression (UnaryExpression expression);
        T VisitAssignmentExpression (AssignmentExpression expression);
        T VisitVariableExpression (VariableExpression expression);
        T VisitLogicalExpression (LogicalExpression expression);
        T VisitCallExpression (CallExpression expression);
        // T VisitGetExpression (GetExpression expression); //expression for accessing field of a class
        // T VisitSetExpression (SetExpression expression); //expression for setting field of a class
        T VisitThisExpression (ThisExpression expression);
        T VisitSuperExpression (SuperExpression expression);
        T VisitListExpression (ListExpression expression);
        T VisitSetExpression (SetExpression expression);
        //expression of element access
        //expression for anon function
    }
    
    public abstract T Accept<T> (IExpressionVisitor<T> visitor);
    
    public class BinaryExpression : Expression
    {
        public readonly Expression left;
        public readonly Token @operator;
        public readonly Expression right;
        
        public BinaryExpression (Expression left, Token @operator, Expression right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }
    
    public class GroupingExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }
    
    public class LiteralExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }
    
    public class UnaryExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
    
    public class AssignmentExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
    
    public class VariableExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }
    
    public class LogicalExpression : Expression
    {
        public readonly Expression left;
        public readonly Token @operator;
        public readonly Expression right;
        
        public LogicalExpression(Expression left, Token @operator, Expression right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpression(this);
        }
    }
    
    public class CallExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }
    
    public class ThisExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitThisExpression(this);
        }
    }
    
    public class SuperExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSuperExpression(this);
        }
    }
    
    public class ListExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitListExpression(this);
        }
    }
    
    public class SetExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetExpression(this);
        }
    }
}