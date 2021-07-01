using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnePass.CLI.Tests
{
    public abstract class TestSetup
    {
        private readonly ServiceProvider _serviceProvider;

        public TestSetup()
        {
            var serviceCollection = Program.BuildServices();
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        protected T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        protected TService GetService<T, TService>()
        {
            var services = _serviceProvider.GetServices<T>();
            return services.OfType<TService>().FirstOrDefault();
        }
    }
}