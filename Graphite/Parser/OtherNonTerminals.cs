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
            public Token? SuperClass { get; private set; }
            public readonly Dictionary<string, Type> fields;
            public readonly Dictionary<Method, Type> methods;

            public Type(Token type, List<Type>? typeArguments)
            {
                this.type = type;
                this.typeArguments = typeArguments;
                fields = [];
                methods = [];
                SuperClass = null;
            }

            public void AddField((string name, Type type) fields)
            {
                this.fields.Add(fields.name, fields.type);
            }

            public void AddMethod((string name, Type returnType, string[] parameterTypes) method)
            {
                methods.Add(new(method.name, method.parameterTypes), method.returnType);
            }

            public void SetSuperClass(Token superClass)
            {
                SuperClass = superClass;
            }

            public bool TryGetMethod(string name, out Type? returnType)
            {
                if (methods.Keys.Any(m => m.Name == name))
                {
                    returnType = methods.First(m => m.Key.Name == name).Value;
                    return true;
                }
                returnType = null;
                return false;
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

        public sealed record Method(string Name, string[] ParameterTypes)
        {
            public bool Equals(Method? other)
            {
                if (other == null)
                    return false;

                if (Name != other.Name)
                    return false;

                if (ParameterTypes.Length != other.ParameterTypes.Length)
                    return false;

                for (int i = 0; i < ParameterTypes.Length; i++)
                {
                    if (ParameterTypes[i] != other.ParameterTypes[i])
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = Name.GetHashCode();
                foreach (var parameterType in ParameterTypes)
                {
                    hashCode = HashCode.Combine(hashCode, parameterType.GetHashCode());
                }
                return hashCode;
            }
        }
    }
}
