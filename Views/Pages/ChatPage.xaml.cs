using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using wpfChat.CustomUserControl;
using wpfChat.Models;
using wpfChat.Services;
using wpfChat.ViewModels.Pages;
using static LLama.Common.ChatHistory;

namespace wpfChat.Views.Pages
{
    public partial class ChatPage : INavigableView<ChatViewModel>
    {
        public ChatViewModel ViewModel { get; }
        private string _rawBuffer = "";        // 原始缓冲区，存储所有未处理的文本
        private string _displayText = "";

        public ChatPage(ChatViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            InitializeStyle();
            viewModel.updateResultEvent += (sender, result) =>
            {
                // 将新文本添加到缓冲区
                _displayText = result;
                // 处理缓冲区，过滤前缀
                //ProcessBuffer();
                // 使用Dispatcher确保在UI线程执行
                Dispatcher.Invoke(() =>
                {
                    const int userPrefixLength = 5;
                    string userPrefix = "User:";
                    string displayText;
                    // 确保有足够的字符进行检查
                    if (_displayText.Length >= userPrefixLength)
                    {
                        string lastChars = _displayText.Substring(_displayText.Length - userPrefixLength);

                        if (lastChars == userPrefix)
                        {
                            // 最后5个字符是"User:"，移除并修剪
                            int endIndex = _displayText.Length - userPrefixLength;
                            displayText = _displayText.Substring(0, endIndex).TrimEnd();
                        }
                        else
                        {
                            // 最后5个字符不是"User:"，保留原始文本
                            displayText = _displayText;
                        }
                    }
                    else
                    {
                        // 文本长度不足5个字符，直接使用原始文本
                        displayText = _displayText;
                    }
                    ChatDisplay.UpdateLastMessage(displayText, DateTime.Now);
                });
            };
            viewModel.clearRichTextBoxEvent += (sender, e) =>
            {
                ChatDisplay.ClearMessages();
                ChatDisplay.AddMessage(AppConfig.InitialPrompt, true, DateTime.Now);
            };
            ChatDisplay.AddMessage(AppConfig.InitialPrompt, true, DateTime.Now);
        }

        private void InitializeStyle()
        {
            AttachmentButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Attach24 };
            ReloadModel.Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowReset24 };
            EndAnswer.Icon = new SymbolIcon { Symbol = SymbolRegular.RecordStop24 };
            //LinkButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Link24 };
            //ImgButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Image24 };
        }

        private void ProcessBuffer()
        {
            // 检查缓冲区中是否包含完整的Assistant:前缀
            const string assistantPrefix = "Assistant";
            const string systemPrefix = "System";
            // 检查是否有完整的Assistant:前缀
            if (_rawBuffer.Contains(assistantPrefix))
            {
                int prefixIndex = _rawBuffer.IndexOf(assistantPrefix, StringComparison.OrdinalIgnoreCase);
                string contentAfterPrefix = _rawBuffer.Substring(prefixIndex + assistantPrefix.Length).TrimStart();
                _displayText = contentAfterPrefix;
                _rawBuffer = "";
            } else if (_rawBuffer.Contains(systemPrefix)) {
                // 检查是否有完整的System:前缀
                int prefixIndex = _rawBuffer.IndexOf(systemPrefix, StringComparison.OrdinalIgnoreCase);
                string contentAfterPrefix = _rawBuffer.Substring(prefixIndex + systemPrefix.Length).TrimStart();
                _displayText = contentAfterPrefix;
                _rawBuffer = "";
            }            
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(
                SendTextBox.Document.ContentStart,
                SendTextBox.Document.ContentEnd);
            string message = textRange.Text;
            // 移除末尾的换行符（处理 \r\n 或 \n）
            if (message.EndsWith("\r\n"))
                message = message.Substring(0, message.Length - 2);
            else if (message.EndsWith("\n"))
                message = message.Substring(0, message.Length - 1);
            string result = string.Empty;
            _displayText = string.Empty;
            _rawBuffer = string.Empty;
            if (!string.IsNullOrEmpty(message))
            {
                ChatDisplay.AddMessage(message, true, DateTime.Now);
                ChatDisplay.AddMessage("", false, DateTime.Now);
                SendTextBox.Document = new FlowDocument();
                result = await ViewModel.SendMessage(message);                
            }

        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = ViewModel.OnPickAttach();
        }        
    }
}
