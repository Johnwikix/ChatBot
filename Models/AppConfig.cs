namespace wpfChat.Models
{
    public class AppConfig
    {
        public static string ConfigurationsFolder { get; set; }

        public static string AppPropertiesFileName { get; set; }

        public static string ModelFolder { get; set; }

        public static string ModelPath { get; set; }
        public static uint ContextSize { get; set; } = 8196;
        public static int TotalLayers { get; set; } = 48;
        public static int MaxTokens { get; set; } = 2048; // 最大token数
        public static string InitialPrompt { get; set; } = "你是一个乐于助人的助手，需要准确回答用户的请求";
        public static string EndPrompt { get; set; } = "有其他问题请告诉我"; // 结束提示
    }
}
