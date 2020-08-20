using Microsoft.Extensions.DependencyInjection;
using OnePass.Infrastructure;
using OnePass.Services.DataAccess;
using OnePass.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace OnePass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceBuilder _serviceBuilder = new ServiceBuilder();

        public bool IsLoggedIn { get; set; } = false;

        public App()
        {
        }

        public T GetService<T>()
        {
            return _serviceBuilder.GetService<T>();
        }
    }
}
