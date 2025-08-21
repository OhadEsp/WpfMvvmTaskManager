using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using System;
using System.Windows;
using TaskManager.Core.Services;
using TaskManager.Infrastructure.Export;
using TaskManager.Infrastructure.Repositories;
using TaskManager.UI.ViewModels;

namespace TaskManager.UI
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITaskRepository, FileTaskRepository>();
            services.AddSingleton<ViewModels.MainViewModel>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IPdfExporter, PdfExporter>();
            services.AddTransient<ViewModels.AddEditTaskViewModel>();
            services.AddTransient<Views.MainWindow>(); // Register MainWindow

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<Views.MainWindow>();
            if (mainWindow != null && mainWindow.DataContext == null)
            {
                mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();
                mainWindow.Show();
            }
            base.OnStartup(e);
        }
    }
}