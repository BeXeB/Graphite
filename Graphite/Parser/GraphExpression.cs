using Graphite.Lexer;

namespace Graphite.Parser;

public abstract class GraphExpression
{
    public interface IGraphExpressionVisitor<T>
    {
        T VisitGraphEdgeExpression (GraphEdgeExpression expression);
        T VisitGraphAddVertexExpression (GraphAddVertexExpression expression);
        T VisitGraphRemoveVertexExpression (GraphRemoveVertexExpression expression);
        T VisitGraphTagExpression (GraphTagExpression expression);
        T VisitGraphReTagExpression (GraphReTagExpression expression);
        T VisitGraphWhileStmt (GraphWhileStmt expression);
        T VisitGraphIfStmt (GraphIfStmt expression);
        T VisitGraphExpressionStmt (GraphExpressionStmt expression);
        T VisitGraphBlockStmt (GraphBlockStmt expression);
        T VisitPredicateOrExpression (PredicateOrExpression expression);
        T VisitPredicateAndExpression (PredicateAndExpression expression);
        T VisitPredicateGroupingExpression (PredicateGroupingExpression expression);
        T VisitPredicateUnaryExpression (PredicateUnaryExpression expression);
        T VisitPredicateLiteralExpression (PredicateLiteralExpression expression);
    }
    
    public abstract T Accept<T> (IGraphExpressionVisitor<T> visitor);
    
    public class GraphEdgeExpression : GraphExpression
    {
        public readonly GraphExpression leftPredicate;
        public readonly Token @operator;
        public readonly GraphExpression rightPredicate;
        public readonly Token weight;
        
        public GraphEdgeExpression (GraphExpression leftPredicate, Token @operator, GraphExpression rightPredicate, Token weight)
        {
            this.leftPredicate = leftPredicate;
            this.@operator = @operator;
            this.rightPredicate = rightPredicate;
            this.weight = weight;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphEdgeExpression(this);
        }
    }
    
    public class GraphAddVertexExpression : GraphExpression
    {
        public readonly Expression tags;
        public readonly Token times;
        
        public GraphAddVertexExpression (Expression tags, Token times)
        {
            this.tags = tags;
            this.times = times;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphAddVertexExpression(this);
        }
    }
    
    public class GraphRemoveVertexExpression : GraphExpression
    {
        public readonly GraphExpression predicate;
        
        public GraphRemoveVertexExpression (GraphExpression predicate)
        {
            this.predicate = predicate;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphRemoveVertexExpression(this);
        }
    }
    
    public class GraphTagExpression : GraphExpression
    {
        public readonly GraphExpression predicate;
        public readonly Token @operator;
        public readonly Expression tags;
        
        public GraphTagExpression (GraphExpression predicate, Token @operator, Expression tags)
        {
            this.predicate = predicate;
            this.@operator = @operator;
            this.tags = tags;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphTagExpression(this);
        }
    }
    
    public class GraphReTagExpression : GraphExpression
    {
        public readonly Token oldTag;
        public readonly Token newTag;
        
        public GraphReTagExpression (Token oldTag, Token newTag)
        {
            this.oldTag = oldTag;
            this.newTag = newTag;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphReTagExpression(this);
        }
    }
    
    public class GraphWhileStmt : GraphExpression
    {
        public readonly Expression condition;
        public readonly GraphExpression body;
        
        public GraphWhileStmt (Expression condition, GraphExpression body)
        {
            this.condition = condition;
            this.body = body;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphWhileStmt(this);
        }
    }
    
    public class GraphIfStmt : GraphExpression
    {
        public readonly Expression condition;
        public readonly GraphExpression thenBranch;
        public readonly GraphExpression? elseBranch;
        
        public GraphIfStmt (Expression condition, GraphExpression thenBranch, GraphExpression? elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphIfStmt(this);
        }
    }
    
    public class GraphExpressionStmt : GraphExpression
    {
        public readonly Statement statement;
        public GraphExpressionStmt (Statement statement)
        {
            this.statement = statement;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphExpressionStmt(this);
        }
    }
    
    public class GraphBlockStmt : GraphExpression
    {
        public readonly List<GraphExpression> statements;
        
        public GraphBlockStmt (List<GraphExpression> statements)
        {
            this.statements = statements;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphBlockStmt(this);
        }
    }
    
    public class PredicateOrExpression : GraphExpression
    {
        public readonly GraphExpression left;
        public readonly Token @operator;
        public readonly GraphExpression right;
        
        public PredicateOrExpression (GraphExpression left, Token @operator, GraphExpression right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateOrExpression(this);
        }
    }
    
    public class PredicateAndExpression : GraphExpression
    {
        public readonly GraphExpression left;
        public readonly Token @operator;
        public readonly GraphExpression right;
        
        public PredicateAndExpression (GraphExpression left, Token @operator, GraphExpression right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateAndExpression(this);
        }
    }
    
    public class PredicateUnaryExpression : GraphExpression
    {
        public readonly Token @operator;
        public readonly GraphExpression right;
        
        public PredicateUnaryExpression (Token @operator, GraphExpression right)
        {
            this.@operator = @operator;
            this.right = right;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateUnaryExpression(this);
        }
    }
    public class PredicateGroupingExpression : GraphExpression
    {
        public readonly GraphExpression expression;
        
        public PredicateGroupingExpression (GraphExpression expression)
        {
            this.expression = expression;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateGroupingExpression(this);
        }
    }
    
    public class PredicateLiteralExpression : GraphExpression
    {
        public readonly Token value;
        
        public PredicateLiteralExpression (Token value)
        {
            this.value = value;
        }
        
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateLiteralExpression(this);
        }
    }
}