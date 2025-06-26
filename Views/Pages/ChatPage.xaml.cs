using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using wpfChat.CustomUserControl;
using wpfChat.Models;
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
            };
            ChatDisplay.AddMessage(AppConfig.InitialPrompt, true, DateTime.Now);
        }

        private void InitializeStyle()
        {
            AttachmentButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Attach24 };
            ReloadModel.Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowReset24 };
            LinkButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Link24 };
            ImgButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Image24 };
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


        //private void UpdateLastestLine(string text) {

        //    int lineIndex = DisplayTextBox.Document.Blocks.Count - 1; // 获取最后一行的索引
        //    // 获取指定索引的段落
        //    Paragraph? paragraph = DisplayTextBox.Document.Blocks.ElementAt(lineIndex) as Paragraph;
        //    if (paragraph == null) return;
        //    // 清空段落现有内容
        //    paragraph.Inlines.Clear();
        //    // 添加新的Run元素
        //    Run newRun = new Run(text);
        //    paragraph.Inlines.Add(newRun);
        //    DisplayTextBox.ScrollToEnd();
        //}

        //private void AddRichTextBoxLine(string text,bool isInital = false) {
        //    TextPointer endPosition = DisplayTextBox.Document.ContentEnd;            
        //    if (DisplayTextBox.Document.Blocks.Count == 1 && isInital) {
        //        // 如果是第一次初始化，清空现有内容
        //        DisplayTextBox.Document.Blocks.Clear();
        //    }
        //    Paragraph newParagraph = new Paragraph();
        //    // 添加文本内容（可设置字体、颜色等样式）
        //    Run run = new Run(text);
        //    newParagraph.Inlines.Add(run);

        //    // 在结尾处插入新段落
        //    DisplayTextBox.Document.Blocks.Add(newParagraph);

        //    // 可选：将光标移到新行末尾
        //    DisplayTextBox.CaretPosition = DisplayTextBox.Document.ContentEnd;
        //    DisplayTextBox.ScrollToEnd();
        //}

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string message =SendTextBox.Text.Trim();
            string result = string.Empty;
            _displayText = string.Empty;
            _rawBuffer = string.Empty;
            if (!string.IsNullOrEmpty(message))
            {
                ChatDisplay.AddMessage(message, true, DateTime.Now);
                ChatDisplay.AddMessage("", false, DateTime.Now);
                SendTextBox.Clear();
                result = await ViewModel.SendMessage(message);                
            }
        }
    }
}
