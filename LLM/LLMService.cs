using LLama.Common;
using LLama;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using wpfChat.Models;
using wpfChat.Services;

namespace wpfChat.LLM
{
    public class LLMService : IDisposable
    {
        private LLamaWeights _model;
        private InteractiveExecutor _executor;
        private ChatHistory _chatHistory;
        private InferenceParams _inferenceParams;
        public EventHandler<string> updateResult;
        public EventHandler<bool> isModelLoaded;
        private string _currentModelPath;
        //private NotificationService _notificationService;

        public LLMService(string modelPath)
        {
            //_notificationService = new NotificationService();
            if (!string.IsNullOrEmpty(modelPath) && File.Exists(modelPath))
            {
                Task.Run(() =>
                {
                    InitializingModel(modelPath);
                });
            }
            else {
                NotificationService.sendToast("模型加载失败", "模型路径为空或文件不存在，请检查配置。");
                //Debug.WriteLine("模型路径为空，无法加载模型。请检查配置。");
            }                  
        }

        // 添加公共方法用于动态切换模型
        public async Task ChangeModelAsync(string newModelPath)
        {
            if (string.IsNullOrEmpty(newModelPath) || !File.Exists(newModelPath))
            {
                NotificationService.sendToast("模型切换失败", "新模型路径无效或文件不存在，请检查配置。");
                //Debug.WriteLine("新模型路径无效，无法加载。");
                return;
            }

            // 释放现有资源
            Dispose();

            _currentModelPath = newModelPath;
            AppConfig.ModelPath = newModelPath; // 更新全局配置

            // 异步加载新模型
            await Task.Run(() => InitializingModel(newModelPath));
        }

        private void InitializingModel(string modelPath) {
            isModelLoaded?.Invoke(this, false); // 模型加载开始事件
            ModelParams parameters = new ModelParams(modelPath)
            {
                ContextSize = AppConfig.ContextSize,
                GpuLayerCount = AppConfig.TotalLayers, // 将所有层都加载到GPU
                UseMemorymap = true   // 使用内存映射，提高加载速度
            };
            // 加载模型
            _model = LLamaWeights.LoadFromFile(parameters);
            var context = _model.CreateContext(parameters);
            _executor = new InteractiveExecutor(context);

            // 初始化对话历史
            _chatHistory = new ChatHistory();
            _chatHistory.AddMessage(AuthorRole.System, "用户与名为智能体的助手交互的对话记录。智能体乐于助人，善良，诚实，善于写作，并且总是能立即准确地回答用户的请求。");
            _chatHistory.AddMessage(AuthorRole.User, "你好智能体");
            _chatHistory.AddMessage(AuthorRole.Assistant, "你好。今天我能帮你什么吗？");

            // 配置推理参数
            _inferenceParams = new InferenceParams()
            {
                MaxTokens = 512,
                AntiPrompts = new List<string> { "User:" }
            };
            Debug.WriteLine("模型加载完成。");
            isModelLoaded?.Invoke(this, true); // 模型加载完成事件
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
                    Debug.WriteLine(response);
                    updateResult?.Invoke(this, response); // 更新结果
                }

                // 将对话添加到历史记录
                _chatHistory.AddMessage(AuthorRole.User, message);
                _chatHistory.AddMessage(AuthorRole.Assistant, response);

                return response;
            }
            catch (Exception ex)
            {
                NotificationService.sendToast("抱歉，生成回复时出错", "Error generating response: " + ex.Message);
                //Console.WriteLine($"Error generating response: {ex.Message}");
                return "抱歉，生成回复时出错。";
            }
        }

        // 重置对话历史
        public void ResetChatHistory()
        {
            _chatHistory.AddMessage(AuthorRole.System, "用户与名为智能体的助手交互的对话记录。智能体乐于助人，善良，诚实，善于写作，并且总是能立即准确地回答用户的请求。");
        }

        // 释放资源
        public void Dispose()
        {
            _executor?.Context?.Dispose();
            _model?.Dispose();
        }
    }
}
