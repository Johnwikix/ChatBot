using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using wpfChat.Data;
using wpfChat.Models;


namespace wpfChat.ViewModels.Pages
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _folderPath = string.Empty;
        [ObservableProperty]
        private ObservableCollection<LLModel> _modelList = new ObservableCollection<LLModel>();

        [ObservableProperty]
        private LLModel? _selectedModel;

        public HomeViewModel()
        {
            // 初始化时加载配置
            initializData();
        }

        private async void initializData() {
            FolderPath = AppConfig.ModelFolder;
            DisplayAndCollectGgufFiles(AppConfig.ModelFolder);
        }

        [RelayCommand]
        private void PickFolder()
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择文件夹",
                Filter = "文件夹|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "文件夹选择"
            };

            // 显示对话框并获取用户选择结果
            if (dialog.ShowDialog() == true)
            {
                // 获取选择的文件夹路径（从文件名中提取，因为文件夹选择没有专门的对话框类）
                string? folderPath = Path.GetDirectoryName(dialog.FileName);
                // 更新UI显示选择的文件夹路径
                FolderPath = folderPath;
                AppConfig.ModelFolder = folderPath;
                DisplayAndCollectGgufFiles(folderPath);
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

        private void DisplayAndCollectGgufFiles(string folderPath)
        {
            try
            {
                // 清空现有列表
                ModelList.Clear();
                // 获取所有 .gguf 文件
                string[] ggufFiles = Directory.GetFiles(folderPath, "*.gguf");
                foreach (string file in ggufFiles)
                {
                    Debug.WriteLine($"找到文件: {file}");
                    // 创建 LLModel 实例并添加到列表
                    LLModel model = new LLModel
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,
                        FileType = "gguf" // 假设所有文件都是 LLM 类型
                    };
                    ModelList.Add(model);
                }
                if (string.IsNullOrEmpty(AppConfig.ModelPath)) {
                    AppConfig.ModelPath = ModelList.Count > 0 ? ModelList[0].Path : string.Empty;
                }
                SelectedModel = ModelList.FirstOrDefault(m => m.Path == AppConfig.ModelPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"访问文件夹时出错: {ex.Message}");
            }
        }
    }
}
