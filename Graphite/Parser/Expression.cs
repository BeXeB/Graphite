﻿using Graphite.Lexer;

namespace Graphite.Parser;

public abstract class Expression : ILanguageConstruct
{
    public int Line { get; set; }
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
        T VisitElementAccessExpression (ElementAccessExpression expression);
        T VisitInstanceExpression (InstanceExpression expression);
        T VisitAnonFunctionExpression (AnonFunctionExpression expression);
    }
    
    public abstract T Accept<T> (IExpressionVisitor<T> visitor);
    
    public class BinaryExpression : Expression
    {
        public readonly Expression left;
        public readonly Token @operator;
        public readonly Expression right;
        
        public BinaryExpression (Expression left, Token @operator, Expression right, int line)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }
    
    public class GroupingExpression : Expression
    {
        public readonly Expression expression;

        public GroupingExpression(Expression expression, int line)
        {
            this.expression = expression;
            Line = line;
        }

        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }
    
    public class LiteralExpression : Expression
    {
        public readonly object? value;
        public readonly Token token;

        public LiteralExpression(object? value, Token token, int line)
        {
            this.value = value;
            this.token = token;
            Line = line;
        }

        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }
    
    public class UnaryExpression : Expression
    {
        public readonly Token @operator;
        public readonly Expression right;

        public UnaryExpression(Token @operator, Expression right, int line)
        {
            this.@operator = @operator;
            this.right = right;
            Line = line;
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
        
        public AssignmentExpression (Token name, Expression value, int line)
        {
            this.name = name;
            this.value = value;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }
    }
    
    public class VariableExpression : Expression
    {
        public readonly Token name;
        
        public VariableExpression (Token name, int line)
        {
            this.name = name;
            Line = line;
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
        
        public LogicalExpression(Expression left, Token @operator, Expression right, int line)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
            Line = line;
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
        
        public CallExpression (Expression callee, List<Expression> arguments, int line)
        {
            this.callee = callee;
            this.arguments = arguments;
            Line = line;
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
        
        public ThisExpression (int line)
        {
            Line = line;
        }
    }
    
    public class SuperExpression : Expression
    {
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSuperExpression(this);
        }
        
        public SuperExpression (int line)
        {
            Line = line;
        }
    }
    
    public class ListExpression : Expression
    {
        public readonly List<Expression> elements;
        
        public ListExpression (List<Expression> elements, int line)
        {
            this.elements = elements;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitListExpression(this);
        }
    }
    
    public class SetExpression : Expression
    {
        public readonly List<Expression> elements;
        
        public SetExpression (List<Expression> elements, int line)
        {
            this.elements = elements;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetExpression(this);
        }
    }
    
    public class GetFieldExpression : Expression
    {
        public readonly Expression obj;
        public readonly Expression field;
        
        public GetFieldExpression (Expression obj, Expression field, int line)
        {
            this.obj = obj;
            this.field = field;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGetFieldExpression(this);
        }
    }
    
    public class SetFieldExpression : Expression
    {
        public readonly Expression obj;
        public readonly Expression field;
        public readonly Expression value;
        
        public SetFieldExpression (Expression obj, Expression field, Expression value, int line)
        {
            this.obj = obj;
            this.field = field;
            this.value = value;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitSetFieldExpression(this);
        }
    }
    
    public class AnonFunctionExpression : Expression
    {
        public readonly Statement.BlockStatement body;
        public readonly OtherNonTerminals.Parameters parameters;
        
        public AnonFunctionExpression (OtherNonTerminals.Parameters parameters, Statement.BlockStatement body, int line)
        {
            this.parameters = parameters;
            this.body = body;
            Line = line;
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
        
        public InstanceExpression (Token className, List<Expression> arguments, int line)
        {
            this.className = className;
            this.arguments = arguments;
            Line = line;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitInstanceExpression(this);
        }
    }
    
    public class ElementAccessExpression : Expression
    {
        public readonly Expression obj;
        public readonly Expression index;
        
        public ElementAccessExpression (Expression obj, Expression index, int line)
        {
            this.obj = obj;
            Line = line;
            this.index = index;
        }
        
        public override T Accept<T> (IExpressionVisitor<T> visitor)
        {
            return visitor.VisitElementAccessExpression(this);
        }
    }
}