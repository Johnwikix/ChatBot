using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;
using wpfChat.Data;
using wpfChat.LLM;
using wpfChat.Models;
using wpfChat.Services;
using wpfChat.ViewModels.Pages;
using wpfChat.ViewModels.Windows;
using wpfChat.Views.Pages;
using wpfChat.Views.Windows;

namespace wpfChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();
                // 1. 首先注册数据库初始化服务，并同时注册为单例供其他服务使用
                services.AddSingleton<DatabaseInitializationService>();
                services.AddHostedService<DatabaseInitializationService>(provider =>
                    provider.GetRequiredService<DatabaseInitializationService>());
                services.AddHostedService<ApplicationHostService>();
                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();
                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();
                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();
                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                // 添加LLM服务;
                services.AddSingleton<LLMService>(_ =>
                    new LLMService(AppConfig.ModelPath));
                services.AddSingleton<HomePage>();
                services.AddSingleton<HomeViewModel>();
                services.AddSingleton<ChatPage>();
                services.AddSingleton<ChatViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
