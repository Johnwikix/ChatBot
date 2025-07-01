using LLama.Common;
using LLama;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;
using wpfChat.Models;
using System.Diagnostics;
using wpfChat.LLM;
using System.IO;
using Microsoft.Win32;
using wpfChat.Services;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace wpfChat.ViewModels.Pages
{
    public partial class ChatViewModel : ObservableObject, INavigationAware
    {
        private readonly LLMService _llmService;
        private bool _isInitialized = false;

        [ObservableProperty]
        private bool _isModelReady = false;

        [ObservableProperty]
        private string _buttonText = "模型加载中...";
        [ObservableProperty]
        private string _modelPath = string.Empty;
        [ObservableProperty]
        private string _reloadBtn = "重载模型";
        private string _attachMessage = string.Empty;
        private bool _isDocAttach = false;

        public EventHandler<string> updateResultEvent;
        public EventHandler clearRichTextBoxEvent;

        public ChatViewModel(LLMService llmService)
        {
            _llmService = llmService;
            _llmService.updateResult += (sender, result) =>
            {
                updateResultEvent?.Invoke(this, result);
            };
            _llmService.isModelLoaded += (sender, isLoaded) =>
            {
                IsModelReady = isLoaded;
                ButtonText = isLoaded ? "发送消息" : "模型加载中...";
                ModelPath = isLoaded ? AppConfig.ModelPath : "未加载模型";
                if(!isLoaded)
                {
                    clearRichTextBoxEvent?.Invoke(this, EventArgs.Empty); // 清空RichTextBox
                }
            };
        }

        public Task OnNavigatedToAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        public async Task<string> SendMessage(string message)
        {
            if(_isDocAttach)
            {
                message = $"{message}\n\n附件内容：\n{_attachMessage}";
                _isDocAttach = false; // 重置附件状态
                _attachMessage = string.Empty; // 清空附件内容
            }
            string response = await _llmService.SendMessageAsync(message);
            return response;
        }
        [RelayCommand]
        private async void OnReloadModel() {
            await _llmService.ChangeModelAsync(AppConfig.ModelPath);
        }
        public string OnPickAttach() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "文档 (*.pdf;*.docx;*.txt;*.md)|*.pdf;*.docx;*.txt;*.md";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "选择文件";
            bool? result = openFileDialog.ShowDialog();
            string filePath = string.Empty;
            if (result == true)
            {
                try
                {
                    filePath = openFileDialog.FileName;
                    _attachMessage = AttachService.GetAttach(filePath);
                    _isDocAttach = true;
                    Debug.WriteLine($"选中的文件内容: {_attachMessage}");
                }
                catch (Exception ex)
                {
                    NotificationService.sendToast("错误", $"选择文件时发生错误: {ex.Message}");
                }
            }
            return filePath;
        }

        public void OpenFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    NotificationService.sendToast("错误", $"文件不存在: {filePath}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.sendToast("错误", $"无法打开文件: {ex.Message}");
            }
        }

    }
}
