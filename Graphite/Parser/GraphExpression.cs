namespace Graphite.Parser;

public abstract class GraphExpression
{
    public interface IGraphExpressionVisitor<T>
    {
        T VisitGraphEdgeExpression (GraphEdgeExpression expression);
        T VisitGraphVertexExpression (GraphVertexExpression expression);
        T VisitGraphTagExpression (GraphTagExpression expression);
        T VisitGraphReTagExpression (GraphReTagExpression expression);
    }
    
    public abstract T Accept<T> (IGraphExpressionVisitor<T> visitor);
    
    public class GraphEdgeExpression : GraphExpression
    {
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphEdgeExpression(this);
        }
    }
    
    public class GraphVertexExpression : GraphExpression
    {
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphVertexExpression(this);
        }
    }
    
    public class GraphTagExpression : GraphExpression
    {
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphTagExpression(this);
        }
    }
    
    public class GraphReTagExpression : GraphExpression
    {
        public override T Accept<T> (IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphReTagExpression(this);
        }
    }
}