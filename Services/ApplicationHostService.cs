using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Wpf.Ui;
using wpfChat.Data;
using wpfChat.LLM;
using wpfChat.ViewModels.Pages;
using wpfChat.Views.Pages;
using wpfChat.Views.Windows;

namespace wpfChat.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        private INavigationWindow _navigationWindow;

        public ApplicationHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 首先等待数据库初始化完成
            await WaitForDatabaseInitializationAsync();
            // 数据库初始化完成后，初始化需要数据库的服务
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await InitializeServicesAsync();
                await HandleActivationAsync();
            });
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task WaitForDatabaseInitializationAsync()
        {
            try
            {
                Debug.WriteLine("等待数据库初始化完成...");
                var dbInitService = _serviceProvider.GetRequiredService<DatabaseInitializationService>();
                await dbInitService.InitializationTask;
                Debug.WriteLine("数据库初始化完成，继续应用程序启动");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"数据库初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化需要等待数据库初始化的服务
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            try
            {
                Debug.WriteLine("开始初始化应用程序服务...");

                // 这些服务的创建会触发它们的构造函数，此时数据库已经初始化完成
                var llmService = _serviceProvider.GetRequiredService<LLMService>();
                var homePage = _serviceProvider.GetRequiredService<HomePage>();
                var homeViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
                var chatPage = _serviceProvider.GetRequiredService<ChatPage>();
                var chatViewModel = _serviceProvider.GetRequiredService<ChatViewModel>();
                var settingsPage = _serviceProvider.GetRequiredService<SettingsPage>();
                var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();

                Debug.WriteLine("所有应用程序服务初始化完成");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"服务初始化失败: {ex.Message}");
                throw;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private async Task HandleActivationAsync()
        {
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _navigationWindow = (
                    _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow
                )!;
                _navigationWindow!.ShowWindow();

                _navigationWindow.Navigate(typeof(Views.Pages.HomePage));
            }

            await Task.CompletedTask;
        }
    }
}
