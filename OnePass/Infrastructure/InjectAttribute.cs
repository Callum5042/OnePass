using System;

namespace OnePass.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {
        public InjectAttribute() { }

        public InjectAttribute(Type @interface) => Interface = @interface;

        public InjectAttribute(Type @interface, Type @class)
        {
            Interface = @interface;
            Class = @class;
        }

        public Type Interface { get; private set; }

        public Type Class { get; private set; }
    }
}
