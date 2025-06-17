namespace wpfChat.Models
{
    public class AppConfig
    {
        public static string ConfigurationsFolder { get; set; }

        public static string AppPropertiesFileName { get; set; }

        public static string ModelFolder { get; set; }

        public static string ModelPath { get; set; }
        public static uint ContextSize { get; set; } = 2048;
        public static int TotalLayers { get; set; } = 45;
    }
}
