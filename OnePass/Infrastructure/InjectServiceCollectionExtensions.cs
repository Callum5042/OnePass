using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace OnePass.Infrastructure
{
    public static class InjectServiceCollectionExtensions
    {
        public static void InjectServicesFromAttribute(this IServiceCollection services)
        {
            var assembly = Assembly.GetCallingAssembly();
            services.InjectServicesFromAttribute(assembly);
        }

        private static ServiceLifetime ResolveLifetime(InjectType injectType)
        {
            switch (injectType)
            {
                case InjectType.Transient: 
                    return ServiceLifetime.Transient;
                case InjectType.Scoped: 
                    return ServiceLifetime.Scoped;
                case InjectType.Singleton: 
                    return ServiceLifetime.Singleton;
                default:
                    throw new ArgumentException("Not supported");
            }

            //return injectType switch
            //{
            //    InjectType.Transient => ServiceLifetime.Transient,
            //    InjectType.Scoped => ServiceLifetime.Scoped,
            //    InjectType.Singleton => ServiceLifetime.Singleton,
            //    _ => throw new ArgumentException("Not supported"),
            //};
        }

        public static void InjectServicesFromAttribute(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(x => x.CustomAttributes.Any(a => a.AttributeType == typeof(InjectAttribute))))
            {
                var attribute = (InjectAttribute)Attribute.GetCustomAttribute(type, typeof(InjectAttribute));
                if (attribute.Interface != null)
                {
                    var implementationType = attribute.Class ?? type;
                    var descriptor = new ServiceDescriptor(attribute.Interface, implementationType, ResolveLifetime(attribute.InjectType));
                    services.Add(descriptor);
                }
                else
                {
                    var descriptor = new ServiceDescriptor(type, type, ResolveLifetime(attribute.InjectType));
                    services.Add(descriptor);
                }
            }
        }
    }
}
