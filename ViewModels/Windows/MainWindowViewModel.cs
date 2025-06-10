using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace wpfChat.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - wpfChat";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Model",
                Icon = new SymbolIcon { Symbol = SymbolRegular.FolderAdd24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
            //new NavigationViewItem()
            //{
            //    Content = "Data",
            //    Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
            //    TargetPageType = typeof(Views.Pages.DataPage)
            //},
            new NavigationViewItem()
            {
                Content = "Chart",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Chat32 },
                TargetPageType = typeof(Views.Pages.DataPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "SET",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
