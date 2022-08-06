using Microsoft.Extensions.DependencyInjection;
using OnePass.Services;
using OnePass.WPF.Services;
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
            InjectConventions(services);
            services.InjectServicesFromAttribute();

            // services.AddTransient<IFileSystem, FileSystem>();
            services.AddSingleton<OnePassData>();
            services.AddTransient<IFileEncoder, Services.FileEncoder>();
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
