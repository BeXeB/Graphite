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
        public readonly Token @operator;
        public readonly Expression right;

        public UnaryExpression(Token @operator, Expression right)
        {
            this.@operator = @operator;
            this.right = right;
        }

        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
    
    public class AssignmentExpression : Expression
    {
        public Token name;
        public Expression value;
        
        public AssignmentExpression (Token name, Expression value)
        {
            this.name = name;
            this.value = value;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
    
    public class VariableExpression : Expression
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
        public readonly Expression callee;
        public readonly List<Expression> arguments;
        
        public CallExpression (Expression callee, List<Expression> arguments)
        {
            this.callee = callee;
            this.arguments = arguments;
        }
        
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
    
    public class GetFieldExpression : Expression
    {
        public readonly Expression obj;
        public readonly Token field;
        
        public GetFieldExpression (Expression obj, Token field)
        {
            this.obj = obj;
            this.field = field;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGetFieldExpression(this);
        }
    }
    
    public class SetFieldExpression : Expression
    {
        public readonly Expression obj;
        public readonly Token field;
        public readonly Expression value;
        
        public SetFieldExpression (Expression obj, Token field, Expression value)
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
    
    public class AnonFunctionExpression : Expression
    {
        public readonly Statement.BlockStatement body;
        public readonly List<Token>? parameters;
        
        public AnonFunctionExpression (List<Token>? parameters, Statement.BlockStatement body)
        {
            this.parameters = parameters;
            this.body = body;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAnonFunctionExpression(this);
        }
    }
    
    public class InstanceExpression : Expression
    {
        public readonly Token className;
        public readonly List<Expression> arguments;
        
        public InstanceExpression (Token className, List<Expression> arguments)
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