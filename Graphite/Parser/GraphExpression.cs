using Graphite.Lexer;

namespace Graphite.Parser;

public abstract class GraphExpression : ILanguageConstruct
{
    public int Line { get; set; }

    public interface IGraphExpressionVisitor<T>
    {
        T VisitGraphEdgeExpression(GraphEdgeExpression expression);
        T VisitGraphAddVertexExpression(GraphAddVertexExpression expression);
        T VisitGraphRemoveVertexExpression(GraphRemoveVertexExpression expression);
        T VisitGraphTagExpression(GraphTagExpression expression);
        T VisitGraphReTagExpression(GraphReTagExpression expression);
        T VisitGraphWhileStmt(GraphWhileStatement expression);
        T VisitGraphIfStmt(GraphIfStatement expression);
        T VisitGraphExpressionStmt(GraphExpressionStatement expression);
        T VisitGraphBlockStatement(GraphBlockStatement expression);
        T VisitAddGraphExpression(AddGraphExpression expression);
        T VisitPredicateOrExpression(PredicateOrExpression expression);
        T VisitPredicateAndExpression(PredicateAndExpression expression);
        T VisitPredicateGroupingExpression(PredicateGroupingExpression expression);
        T VisitPredicateUnaryExpression(PredicateUnaryExpression expression);
        T VisitPredicateLiteralExpression(PredicateLiteralExpression expression);
    }

    public abstract T Accept<T>(IGraphExpressionVisitor<T> visitor);

    public class GraphEdgeExpression : GraphExpression
    {
        public readonly GraphExpression leftPredicate;
        public readonly Token @operator;
        public readonly GraphExpression rightPredicate;
        public readonly Expression weight;

        public GraphEdgeExpression(GraphExpression leftPredicate, Token @operator, GraphExpression rightPredicate,
            Expression weight, int line)
        {
            this.leftPredicate = leftPredicate;
            this.@operator = @operator;
            this.rightPredicate = rightPredicate;
            this.weight = weight;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphEdgeExpression(this);
        }
    }

    public class GraphAddVertexExpression : GraphExpression
    {
        public readonly Expression tags;
        public readonly Expression times;

        public GraphAddVertexExpression(Expression tags, Expression times, int line)
        {
            this.tags = tags;
            this.times = times;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphAddVertexExpression(this);
        }
    }

    public class GraphRemoveVertexExpression : GraphExpression
    {
        public readonly GraphExpression predicate;

        public GraphRemoveVertexExpression(GraphExpression predicate, int line)
        {
            this.predicate = predicate;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphRemoveVertexExpression(this);
        }
    }

    public class GraphTagExpression : GraphExpression
    {
        public readonly GraphExpression predicate;
        public readonly Token @operator;
        public readonly Expression tags;

        public GraphTagExpression(GraphExpression predicate, Token @operator, Expression tags, int line)
        {
            this.predicate = predicate;
            this.@operator = @operator;
            this.tags = tags;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphTagExpression(this);
        }
    }

    public class GraphReTagExpression : GraphExpression
    {
        public readonly Expression oldTag;
        public readonly Expression newTag;

        public GraphReTagExpression(Expression oldTag, Expression newTag, int line)
        {
            this.oldTag = oldTag;
            this.newTag = newTag;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphReTagExpression(this);
        }
    }

    public class GraphWhileStatement : GraphExpression
    {
        public readonly Expression condition;
        public readonly GraphExpression body;

        public GraphWhileStatement(Expression condition, GraphExpression body, int line)
        {
            this.condition = condition;
            this.body = body;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphWhileStmt(this);
        }
    }

    public class GraphIfStatement : GraphExpression
    {
        public readonly Expression condition;
        public readonly GraphExpression thenBranch;
        public readonly GraphExpression? elseBranch;

        public GraphIfStatement(Expression condition, GraphExpression thenBranch, GraphExpression? elseBranch, int line)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphIfStmt(this);
        }
    }

    public class GraphExpressionStatement : GraphExpression
    {
        public readonly Statement statement;

        public GraphExpressionStatement(Statement statement, int line)
        {
            this.statement = statement;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphExpressionStmt(this);
        }
    }

    public class GraphBlockStatement : GraphExpression
    {
        public readonly List<GraphExpression> statements;

        public GraphBlockStatement(List<GraphExpression> statements, int line)
        {
            this.statements = statements;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitGraphBlockStatement(this);
        }
    }

    public class PredicateOrExpression : GraphExpression
    {
        public readonly GraphExpression left;
        public readonly Token @operator;
        public readonly GraphExpression right;

        public PredicateOrExpression(GraphExpression left, Token @operator, GraphExpression right, int line)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateOrExpression(this);
        }
    }

    public class PredicateAndExpression : GraphExpression
    {
        public readonly GraphExpression left;
        public readonly Token @operator;
        public readonly GraphExpression right;

        public PredicateAndExpression(GraphExpression left, Token @operator, GraphExpression right, int line)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateAndExpression(this);
        }
    }

    public class PredicateUnaryExpression : GraphExpression
    {
        public readonly Token @operator;
        public readonly GraphExpression right;

        public PredicateUnaryExpression(Token @operator, GraphExpression right, int line)
        {
            this.@operator = @operator;
            this.right = right;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateUnaryExpression(this);
        }
    }

    public class PredicateGroupingExpression : GraphExpression
    {
        public readonly GraphExpression expression;

        public PredicateGroupingExpression(GraphExpression expression, int line)
        {
            this.expression = expression;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateGroupingExpression(this);
        }
    }

    public class PredicateLiteralExpression : GraphExpression
    {
        public readonly Expression expression;

        public PredicateLiteralExpression(Expression expression, int line)
        {
            this.expression = expression;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitPredicateLiteralExpression(this);
        }
    }

    public class AddGraphExpression : GraphExpression
    {
        public readonly Token otherGraph;

        public AddGraphExpression(Token otherGraph, int line)
        {
            this.otherGraph = otherGraph;
            Line = line;
        }

        public override T Accept<T>(IGraphExpressionVisitor<T> visitor)
        {
            return visitor.VisitAddGraphExpression(this);
        }
    }
}