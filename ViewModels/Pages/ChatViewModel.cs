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
using System.Collections.ObjectModel;
using System.Net.Mail;
using Attachment = wpfChat.Models.Attachment;

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
        [ObservableProperty]
        private ObservableCollection<Attachment> _attachments = new ObservableCollection<Attachment>();

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
            AddAttachmentContent();            
            if (_isDocAttach)
            {
                message = $"{message}\n\n{_attachMessage}";
                Debug.WriteLine($"message: {message}");
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
                }
                catch (Exception ex)
                {
                    NotificationService.sendToast("错误", $"选择文件时发生错误: {ex.Message}");
                }
            }
            Attachments.Add(new Attachment
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
            });
            return filePath;
        }

        [RelayCommand]
        private void OnFileOpen(string parameter)
        {
            Debug.WriteLine(parameter);
            try
            {
                if (File.Exists(parameter))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = parameter,
                        UseShellExecute = true
                    });
                }
                else
                {
                    NotificationService.sendToast("错误", $"文件不存在: {parameter}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.sendToast("错误", $"无法打开文件: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OnEndAnswer()
        {
            _llmService.StopAnswer();
        }
        [RelayCommand]
        private void OnClearAttachment() {
            Attachments.Clear(); // 清空附件列表
            _isDocAttach = false; // 重置附件状态
            _attachMessage = string.Empty; // 清空附件内容
        }

        private void AddAttachmentContent()
        {
            _isDocAttach = true; // 设置附件状态为已附加
            if (Attachments != null && Attachments.Count > 0) {
                _attachMessage += "以下是文档内容:";
                foreach (Attachment attachment in Attachments)
                {
                    if (File.Exists(attachment.FilePath))
                    {
                        try
                        {
                            _attachMessage += AttachService.GetAttach(attachment.FilePath) + "\n\n";
                        }
                        catch (Exception ex)
                        {
                            NotificationService.sendToast("错误", $"读取附件内容时发生错误: {ex.Message}");
                        }
                    }
                    else
                    {
                        NotificationService.sendToast("错误", $"附件文件不存在: {attachment.FilePath}");
                    }
                }
            }           
        }
    }
}
