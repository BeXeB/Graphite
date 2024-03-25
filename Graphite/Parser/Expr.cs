using Graphite.Lexer;

namespace Graphite.Parser;

public abstract class Expr
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
        T VisitGetFieldExpression (GetFieldExpression expression); //expression for accessing field of a class
        T VisitSetFieldExpression (SetFieldExpression expression); //expression for setting field of a class
        T VisitThisExpression (ThisExpression expression);
        T VisitSuperExpression (SuperExpression expression);
        T VisitListExpression (ListExpression expression);
        T VisitSetExpression (SetExpression expression);
        //expression of element access
        T VisitInstanceExpression (InstanceExpression expression);
        T VisitAnonFunctionExpression (AnonFunctionExpression expression);
    }
    
    public abstract T Accept<T> (IExpressionVisitor<T> visitor);
    
    public class BinaryExpression : Expr
    {
        public readonly Expr left;
        public readonly Token @operator;
        public readonly Expr right;
        
        public BinaryExpression (Expr left, Token @operator, Expr right)
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
    
    public class GroupingExpression : Expr
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }
    
    public class LiteralExpression : Expr
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }
    
    public class UnaryExpression : Expr
    {
        public readonly Token @operator;
        public readonly Expr right;

        public UnaryExpression(Token @operator, Expr right)
        {
            this.@operator = @operator;
            this.right = right;
        }

        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
    
    public class AssignmentExpression : Expr
    {
        public Token name;
        public Expr value;
        
        public AssignmentExpression (Token name, Expr value)
        {
            this.name = name;
            this.value = value;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
    
    public class VariableExpression : Expr
    {
        public readonly Token name;
        
        public VariableExpression (Token name)
        {
            this.name = name;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }
    
    public class LogicalExpression : Expr
    {
        public readonly Expr left;
        public readonly Token @operator;
        public readonly Expr right;
        
        public LogicalExpression(Expr left, Token @operator, Expr right)
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
    
    public class CallExpression : Expr
    {
        public readonly Expr callee;
        public readonly List<Expr> arguments;
        
        public CallExpression (Expr callee, List<Expr> arguments)
        {
            this.callee = callee;
            this.arguments = arguments;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }
    
    public class ThisExpression : Expr
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitThisExpression(this);
        }
    }
    
    public class SuperExpression : Expr
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSuperExpression(this);
        }
    }
    
    public class ListExpression : Expr
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitListExpression(this);
        }
    }
    
    public class SetExpression : Expr
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetExpression(this);
        }
    }
    
    public class GetFieldExpression : Expr
    {
        public readonly Expr obj;
        public readonly Token field;
        
        public GetFieldExpression (Expr obj, Token field)
        {
            this.obj = obj;
            this.field = field;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGetFieldExpression(this);
        }
    }
    
    public class SetFieldExpression : Expr
    {
        public readonly Expr obj;
        public readonly Token field;
        public readonly Expr value;
        
        public SetFieldExpression (Expr obj, Token field, Expr value)
        {
            this.obj = obj;
            this.field = field;
            this.value = value;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetFieldExpression(this);
        }
    }
    
    public class AnonFunctionExpression : Expr
    {
        public readonly Statement.BlockStatement body;
        public readonly OtherNonTerminals.Parameters parameters;
        
        public AnonFunctionExpression (OtherNonTerminals.Parameters parameters, Statement.BlockStatement body)
        {
            this.parameters = parameters;
            this.body = body;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAnonFunctionExpression(this);
        }
    }
    
    public class InstanceExpression : Expr
    {
        public readonly Token className;
        public readonly List<Expr> arguments;
        
        public InstanceExpression (Token className, List<Expr> arguments)
        {
            this.className = className;
            this.arguments = arguments;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitInstanceExpression(this);
        }
    }
}