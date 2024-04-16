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
            public readonly List<Type>? typeArguments;
            
            //for class types
            public string SuperClass { get; private set; }
            public readonly List<(string, Type)>? fields;
            public readonly List<(string, Type)>? methods;

            public Type(Token type, List<Type>? typeArguments)
            {
                this.type = type;
                this.typeArguments = typeArguments;
                fields = new List<(string, Type)>();
                methods = new List<(string, Type)>();
            }
            
            public void AddField((string, Type) fields)
            {
                this.fields?.Add(fields);
            }
            
            public void AddMethod((string, Type) methods)
            {
                this.methods?.Add(methods);
            }
            
            public void SetSuperClass(string superClass)
            {
                SuperClass = superClass;
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
