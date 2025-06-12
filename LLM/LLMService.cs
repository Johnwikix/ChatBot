using LLama.Common;
using LLama;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.LLM
{
    public class LLMService : IDisposable
    {
        private readonly LLamaWeights _model;
        private readonly InteractiveExecutor _executor;
        private readonly ChatHistory _chatHistory;
        private readonly InferenceParams _inferenceParams;

        public LLMService(string modelPath)
        {
            // 配置模型参数
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 5,
            };

            // 加载模型
            _model = LLamaWeights.LoadFromFile(parameters);
            var context = _model.CreateContext(parameters);
            _executor = new InteractiveExecutor(context);

            // 初始化对话历史
            _chatHistory = new ChatHistory();
            _chatHistory.AddMessage(AuthorRole.System, "用户与名为智能体的助手交互的对话记录。Bob乐于助人，善良，诚实，善于写作，并且总是能立即准确地回答用户的请求。");
            _chatHistory.AddMessage(AuthorRole.User, "你好智能体");
            _chatHistory.AddMessage(AuthorRole.Assistant, "你好。今天我能帮你什么吗？");

            // 配置推理参数
            _inferenceParams = new InferenceParams()
            {
                MaxTokens = 256,
                AntiPrompts = new List<string> { "User:" }
            };
        }

        // 发送消息并获取回复
        public async Task<string> SendMessageAsync(string message)
        {
            try
            {
                string response = string.Empty;
                var session = new ChatSession(_executor, _chatHistory);

                await foreach (var text in session.ChatAsync(
                    new ChatHistory.Message(AuthorRole.User, message),
                    _inferenceParams))
                {
                    response += text;
                }

                // 将对话添加到历史记录
                _chatHistory.AddMessage(AuthorRole.User, message);
                _chatHistory.AddMessage(AuthorRole.Assistant, response);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating response: {ex.Message}");
                return "抱歉，生成回复时出错。";
            }
        }

        // 重置对话历史
        public void ResetChatHistory()
        {
            _chatHistory.AddMessage(AuthorRole.System, "用户与名为智能体的助手交互的对话记录。Bob乐于助人，善良，诚实，善于写作，并且总是能立即准确地回答用户的请求。");
        }

        // 释放资源
        public void Dispose()
        {
            _executor?.Context?.Dispose();
            _model?.Dispose();
        }
    }
}
