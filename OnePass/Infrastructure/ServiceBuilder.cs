﻿using Microsoft.Extensions.DependencyInjection;
using OnePass.Services;
using System;
using System.Linq;
using System.Reflection;

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
            return _serviceProvider.GetService<T>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            InjectServices(services);
            services.AddSingleton<ISettingsMonitor, SettingsMonitor>();
        }

        private void InjectServices(IServiceCollection services)
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
    }
}
