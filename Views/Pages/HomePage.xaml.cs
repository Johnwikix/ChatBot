using System.Diagnostics;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using wpfChat.Data;
using wpfChat.Models;
using wpfChat.ViewModels.Pages;

namespace wpfChat.Views.Pages
{
    public partial class HomePage : INavigableView<HomeViewModel>
    {
        public HomeViewModel ViewModel { get; }

        public HomePage(HomeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            InitializeStyle();
        }
        private void InitializeStyle() {
            PickFolder.Icon = new SymbolIcon { Symbol = SymbolRegular.FolderAdd24 };
        }

        private void ModelListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItem is LLModel selectedModel)
            {
                // 处理选中项
                Debug.WriteLine($"选中模型: {selectedModel.Name}");
                if (!selectedModel.Path.Equals(AppConfig.ModelPath)) {
                    ViewModel.ChangeModel(AppConfig.ModelPath);
                }
                // 更新应用配置
                AppConfig.ModelPath = selectedModel.Path;

                // 可以在这里添加其他处理逻辑
                SaveConfig saveConfig = new SaveConfig
                {
                    ModelFolder = AppConfig.ModelFolder,
                    ModelPath = AppConfig.ModelPath
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
            }
        }
    }
}
