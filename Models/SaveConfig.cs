using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.Models
{
    public class SaveConfig
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string ModelFolder { get; set; } = "Models";
        public string ModelPath { get; set; }
        public uint ContextSize { get; set; } = 2048;
        public int TotalLayers { get; set; } = 45;
        public int MaxTokens { get; set; } = 2048;
        public string InitialPrompt { get; set; } = "你是一个乐于助人的助手，需要准确回答用户的请求";
        public string EndPrompt { get; set; } = "有其他问题请告诉我"; // 结束提示
        public string SelectPromptName { get; set; } = "文档处理";
        public float Temperature { get; set; } = 0.75f; // 温度.
        public float RepeatPenalty { get; set; } = 1.1f; // 重复惩罚
        public int TopK { get; set; } = 40; // TopK采样
        public float TopP { get; set; } = 0.9f; // TopP采样,1f表示关闭TopP采样
        public float TypicalP { get; set; } = 1f; // Typical采样与TopP互斥，1f表示关闭典型采样
        public bool PreventEOS { get; set; } = false; // false表示允许模型生成结束符
        public int MinKeep { get; set; } = 1;
    }
}
