using LLama.Common;
using LLama;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;
using wpfChat.Models;
using System.Diagnostics;
using wpfChat.LLM;
using System.IO;

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
            string response = await _llmService.SendMessageAsync(message);
            return response;
        }
    }
}
