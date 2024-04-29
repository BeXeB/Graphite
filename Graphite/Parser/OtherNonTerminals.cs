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
            public readonly Token type;
            public readonly List<Type>? typeArguments;
            
            //for class types
            public Token? SuperClass { get; private set; } //TODO Change to Type
            public readonly Dictionary<string, Type> fields; //TODO use variable type here
            public readonly Dictionary<string, Type> methods; //TODO use function type here
            public bool IsDummyType { get; private set; }

            public Type(Token type, List<Type>? typeArguments, bool isDummyType = false)
            {
                this.type = type;
                this.typeArguments = typeArguments;
                fields = new Dictionary<string, Type>(); //TODO use variable type here
                methods = new Dictionary<string, Type>(); //TODO use function type here
                SuperClass = null;
                IsDummyType = isDummyType;
            }
            
            public void AddField((string name, Type type) field)
            {
                fields.Add(field.name, field.type);
            }
            
            public void AddMethod((string name, Type type) method)
            {
                methods.Add(method.name, method.type);
            }
            
            public bool HasMember(string memberName)
            {
                return fields.ContainsKey(memberName) || methods.ContainsKey(memberName);
            }
            
            public void SetSuperClass(Token superClass)
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
