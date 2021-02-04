using Microsoft.Extensions.DependencyInjection;
using OnePass.Services;
using OnePass.Services.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace OnePass.Infrastructure
{
    public class ServiceBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceBuilder()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public T GetService<T>()
        {
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetService<T>();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            InjectServices(services);
            InjectConventions(services);

            services.AddSingleton<ISettingsMonitor, SettingsMonitor>();
            services.AddSingleton<OnePassRepository, OnePassRepository>();
        }

        private static void InjectServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes().Where(x => x.CustomAttributes.Any(a => a.AttributeType == typeof(InjectAttribute))))
            {
                var attribute = (InjectAttribute)Attribute.GetCustomAttribute(type, typeof(InjectAttribute));
                if (attribute.Interface != null)
                {
                    if (attribute.Class != null)
                    {
                        services.AddTransient(attribute.Interface, attribute.Class);
                    }
                    else
                    {
                        services.AddTransient(attribute.Interface, type);
                    }
                }
                else
                {
                    services.AddTransient(type);
                }
            }
        }

        private static void InjectConventions(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Window)) || x.IsSubclassOf(typeof(Page))))
            {
                services.AddTransient(type);
            }
        }
    }
}
