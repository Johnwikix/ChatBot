using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.Models
{
    /// <summary>
    /// 聊天消息数据模型
    /// </summary>
    public class ChatMessage:ObservableObject
    {
        public string _content;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
        public bool _isFromMe;
        public bool IsFromMe
        {
            get => _isFromMe;
            set => SetProperty(ref _isFromMe, value);
        }
        public DateTime Timestamp { get; set; }
    }
}
