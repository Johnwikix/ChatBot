using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
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
                _displayText = result;
                Dispatcher.Invoke(() =>
                {
                    const int userPrefixLength = 5;
                    string userPrefix = "User:";
                    string displayText;
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
            ClearAttachmentButton.Icon = new SymbolIcon { Symbol = SymbolRegular.SubtractCircle24 };
            //LinkButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Link24 };
            //ImgButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Image24 };
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(
                SendTextBox.Document.ContentStart,
                SendTextBox.Document.ContentEnd);
            string message = textRange.Text;
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
