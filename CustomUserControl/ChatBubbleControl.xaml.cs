using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpfChat.Models;

namespace wpfChat.CustomUserControl
{
    public partial class ChatBubbleControl : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }

        public ChatBubbleControl()
        {
            InitializeComponent();
            Messages = new ObservableCollection<ChatMessage>();
            DataContext = this;
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="isFromMe">是否为自己发送的消息</param>
        /// <param name="timestamp">时间戳，不指定则使用当前时间</param>
        public void AddMessage(string content, bool isFromMe, DateTime? timestamp = null)
        {
            var message = new ChatMessage
            {
                Content = content,
                IsFromMe = isFromMe,
                Timestamp = timestamp ?? DateTime.Now
            };

            Messages.Add(message);

            // 自动滚动到底部
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
        }

        public void UpdateLastMessage(string newContent, DateTime? timestamp = null)
        {
            if (Messages.Count == 0)
                return;

            var lastMessage = Messages[Messages.Count - 1];
            lastMessage.Content = newContent;

            if (timestamp.HasValue)
                lastMessage.Timestamp = timestamp.Value;

            // 通知UI更新
            OnPropertyChanged(nameof(Messages));

            // 自动滚动到底部
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
        }

        public void ClearMessages()
        {
            Messages.Clear();
        }

        public void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }

        public void ScrollToTop()
        {
            ChatScrollViewer.ScrollToTop();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
