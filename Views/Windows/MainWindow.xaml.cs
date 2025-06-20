using System.Diagnostics;
using System.Windows.Navigation;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using wpfChat.Data;
using wpfChat.Models;
using wpfChat.ViewModels.Windows;

namespace wpfChat.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;
            SystemThemeWatcher.Watch(this);
            InitializeComponent();            
            SetPageService(navigationViewPageProvider);
            navigationService.SetNavigationControl(RootNavigation);
            this.Closed += (sender, args) =>
            {
                SaveConfig saveConfig = new SaveConfig
                {
                    ModelFolder = AppConfig.ModelFolder,
                    ModelPath = AppConfig.ModelPath,
                    ContextSize = AppConfig.ContextSize,
                    TotalLayers = AppConfig.TotalLayers,
                    MaxTokens = AppConfig.MaxTokens,
                    InitialPrompt = AppConfig.InitialPrompt,
                    EndPrompt = AppConfig.EndPrompt
                };
                DataService.SaveAppConfigAsync(saveConfig).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.WriteLine($"保存配置时出错: {task.Exception?.Message}");
                    }
                    else
                    {
                        Debug.WriteLine("配置已成功保存。");
                    }
                });
            };


        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
