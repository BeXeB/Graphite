using Graphite.Lexer;

namespace Graphite.Parser
{
    public abstract class OtherNonTerminals
    {
        public interface IOtherNonTerminalsVisitor<T>
        {
            T VisitType(Type type);
            T VisitParameters(Parameters parameters);
        }
        public abstract T Accept<T>(IOtherNonTerminalsVisitor<T> visitor);

        public class Type : OtherNonTerminals
        {
            public readonly Token? type;
            public readonly List<Type> typeArguments;

            public Type(Token type, List<Type> typeArguments)
            {
                this.type = type;
                this.typeArguments = typeArguments;
            }

            public override T Accept<T>(IOtherNonTerminalsVisitor<T> visitor)
            {
                return visitor.VisitType(this);
            }
        }

        public class Parameters : OtherNonTerminals
        {
            public readonly List<(Type, Token)> parameters;

            public Parameters(List<(Type, Token)> parameters)
            {
                this.parameters = parameters;
            }

            public override T Accept<T>(IOtherNonTerminalsVisitor<T> visitor)
            {
                return visitor.VisitParameters(this);
            }
        }
    }
}
