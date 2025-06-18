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
            //if (!_isInitialized)
            //    InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        //private void InitializeViewModel()
        //{
        //    var random = new Random();
        //    var colorCollection = new List<DataColor>();

        //    for (int i = 0; i < 8192; i++)
        //        colorCollection.Add(
        //            new DataColor
        //            {
        //                Color = new SolidColorBrush(
        //                    Color.FromArgb(
        //                        (byte)200,
        //                        (byte)random.Next(0, 250),
        //                        (byte)random.Next(0, 250),
        //                        (byte)random.Next(0, 250)
        //                    )
        //                )
        //            }
        //        );

        //    Colors = colorCollection;

        //    _isInitialized = true;
        //}

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
        [RelayCommand]
        private void OnPickAttach() {
            // 创建文件打开对话框
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置筛选器，允许多种文件类型
            openFileDialog.Filter = "文档 (*.pdf;*.docx;*.txt;*.md)|*.pdf;*.docx;*.txt;*.md";

            // 允许多选
            openFileDialog.Multiselect = false;

            // 设置对话框标题
            openFileDialog.Title = "选择文件";

            // 显示对话框并获取用户操作结果
            bool? result = openFileDialog.ShowDialog();

            // 处理用户选择
            // 处理用户选择
            if (result == true)
            {
                try
                {
                    // 获取选中的文件路径
                    string filePath = openFileDialog.FileName;
                    // 显示成功消息
                    _attachMessage = AttachService.GetAttach(filePath);
                    _isDocAttach = true;
                    Debug.WriteLine($"选中的文件内容: {_attachMessage}");
                }
                catch (Exception ex)
                {
                    // 显示错误消息
                    NotificationService.sendToast("错误", $"选择文件时发生错误: {ex.Message}");
                }
            }
        }
    }
}
