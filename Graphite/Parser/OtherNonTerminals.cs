﻿using Graphite.Lexer;

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
            public Token? SuperClass { get; private set; }
            public Dictionary<string, Type> fields;
            public Dictionary<string, Type> methods;
            public bool IsDummyType { get; private set; }

            public Type(Token type, List<Type>? typeArguments, bool isDummyType = false)
            {
                this.type = type;
                this.typeArguments = typeArguments;
                fields = new Dictionary<string, Type>();
                methods = new Dictionary<string, Type>();
                SuperClass = null;
                this.IsDummyType = isDummyType;
            }
            
            public void AddField((string name, Type type) fields)
            {
                this.fields.Add(fields.name, fields.type);
            }
            
            public void AddMethod((string name, Type type) methods)
            {
                this.methods.Add(methods.name, methods.type);
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
