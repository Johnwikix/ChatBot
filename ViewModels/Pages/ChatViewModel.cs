using LLama.Common;
using LLama;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;
using wpfChat.Models;
using System.Diagnostics;

namespace wpfChat.ViewModels.Pages
{
    public partial class ChatViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private IEnumerable<DataColor> _colors;
        public EventHandler<string> updateResultEvent;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            var random = new Random();
            var colorCollection = new List<DataColor>();

            for (int i = 0; i < 8192; i++)
                colorCollection.Add(
                    new DataColor
                    {
                        Color = new SolidColorBrush(
                            Color.FromArgb(
                                (byte)200,
                                (byte)random.Next(0, 250),
                                (byte)random.Next(0, 250),
                                (byte)random.Next(0, 250)
                            )
                        )
                    }
                );

            Colors = colorCollection;

            _isInitialized = true;
        }

        public async Task<string> SendMessage(string message)
        {
            return await StartModel(message);
        }

        private async Task<string> StartModel(string input) {

            string modelPath = @"D:\LLmModel\llama-2-7b.Q4_0.gguf"; // change it to your own model path.

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024, // The longest length of chat as memory.
                GpuLayerCount = 5 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
            };
            using var model = LLamaWeights.LoadFromFile(parameters);
            using var context = model.CreateContext(parameters);
            var executor = new InteractiveExecutor(context);
            var chatHistory = new ChatHistory();
            chatHistory.AddMessage(AuthorRole.System, "用户与名为智能体的助手交互的对话记录。Bob乐于助人，善良，诚实，善于写作，并且总是能立即准确地回答用户的请求。");
            chatHistory.AddMessage(AuthorRole.User, "你好智能体");
            chatHistory.AddMessage(AuthorRole.Assistant, "你好。今天我能帮你什么吗？");

            ChatSession session = new(executor, chatHistory);

            InferenceParams inferenceParams = new InferenceParams()
            {
                MaxTokens = 256, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
                AntiPrompts = new List<string> { "User:" } // Stop generation once antiprompts appear.
            };

            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.Write("The chat session has started.\nUser: ");
            //Console.ForegroundColor = ConsoleColor.Green;
            string userInput = input;
            string response = string.Empty;
            await foreach (var text in session.ChatAsync(
                        new ChatHistory.Message(AuthorRole.User, userInput),
                        inferenceParams))
            {                
                response += text;
                updateResultEvent?.Invoke(this, response);
            }

            return response;

            //while (userInput != "exit")
            //{
            //    await foreach ( // Generate the response streamingly.
            //        var text
            //        in session.ChatAsync(
            //            new ChatHistory.Message(AuthorRole.User, userInput),
            //            inferenceParams))
            //    {
            //        Console.ForegroundColor = ConsoleColor.White;
            //        Console.Write(text);
            //    }
            //    Console.ForegroundColor = ConsoleColor.Green;
            //    userInput = Console.ReadLine() ?? "";
            //}
        }
    }
}
