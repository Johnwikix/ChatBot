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
using LLama.Sampling;

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
            _chatHistory.AddMessage(AuthorRole.System, $"{AppConfig.InitialPrompt},在回答结束时加上结束语：{AppConfig.EndPrompt}");

            // 配置推理参数
            _inferenceParams = new InferenceParams()
            {
                MaxTokens = AppConfig.MaxTokens,
                AntiPrompts = new List<string> {
                    "User:",           // 用户提示符
                    "\nUser:",         // 换行后的用户提示符
                    "Human:",          // 人类提示符
                    "\nHuman:"         // 换行后的人类提示符
                },
                SamplingPipeline = new DefaultSamplingPipeline{
                    Temperature = 0.75f,         // 降低随机性，提高稳定性
                    RepeatPenalty = 1.1f,       // 适度的重复惩罚
                    TopK = 40,                  // 添加TopK采样
                    TopP = 0.9f,                // 添加TopP采样
                    PreventEOS = false,         // 允许模型生成结束符
                    MinKeep = 1,               // 至少保留1个候选token   
                }
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
                int tokenCount = 0;
                await foreach (var text in session.ChatAsync(
                    new ChatHistory.Message(AuthorRole.User, message),
                    _inferenceParams))
                {
                    response += text;
                    tokenCount++;
                    Debug.WriteLine(response);
                    updateResult?.Invoke(this, response); // 更新结果
                    //检查是否达到合理的停止条件
                    if (tokenCount >= AppConfig.MaxTokens)
                    {
                        Debug.WriteLine("达到最大token限制，停止生成");
                        break;
                    }

                    // 检查是否包含自然结束标记
                    if (IsNaturalEndOfResponse(response))
                    {
                        Debug.WriteLine("检测到自然结束，停止生成");
                        break;
                    }
                }

                response = CleanResponse(response);

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

        private bool IsNaturalEndOfResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // 检查是否以句号、问号、感叹号等结束
            //string trimmed = response.TrimEnd();
            //if (trimmed.EndsWith(".") || trimmed.EndsWith("。") ||
            //    trimmed.EndsWith("!") || trimmed.EndsWith("！") ||
            //    trimmed.EndsWith("?") || trimmed.EndsWith("？"))
            //{
            //    // 确保不是在句子中间
            //    string[] lines = response.Split('\n');
            //    string lastLine = lines[lines.Length - 1].Trim();

            //    // 如果最后一行看起来是完整的句子，且不是很短
            //    if (lastLine.Length > 10 && !lastLine.EndsWith("..."))
            //    {
            //        return true;
            //    }
            //}

            // 检查是否包含明显的结束模式
            string[] endPatterns = {
                "希望这能帮到你",
                "有其他问题请告诉我",
                "还有什么需要帮助的吗",
                "如果你有其他问题",
                "Hope this helps",
                "Let me know if you need",
                "Is there anything else",
                AppConfig.EndPrompt
            };

            foreach (string pattern in endPatterns)
            {
                if (response.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private string CleanResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return response;

            // 移除可能的不完整句子
            string[] lines = response.Split('\n');
            List<string> cleanLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // 跳过空行
                if (string.IsNullOrEmpty(line))
                    continue;

                // 如果是最后一行且看起来不完整，可能需要移除
                if (i == lines.Length - 1)
                {
                    // 检查最后一行是否看起来完整
                    if (line.Length < 5 ||
                        (!line.EndsWith(".") && !line.EndsWith("。") &&
                         !line.EndsWith("!") && !line.EndsWith("！") &&
                         !line.EndsWith("?") && !line.EndsWith("？") &&
                         !line.EndsWith("\"") &&
                         line.Contains(" "))
                       )
                    {
                        // 如果看起来不完整，检查是否应该保留
                        if (!ShouldKeepIncompleteResponse(line))
                        {
                            continue; // 跳过这一行
                        }
                    }
                }

                cleanLines.Add(line);
            }

            return string.Join("\n", cleanLines).Trim();
        }

        // 判断是否应该保留看起来不完整的回复
        private bool ShouldKeepIncompleteResponse(string line)
        {
            // 如果回复很短，可能是完整的简短回答
            if (line.Length <= 20)
                return true;

            // 如果包含关键词，可能是有意义的回复
            string[] meaningfulWords = { "是的", "不是", "可以", "不行", "好的", "当然", "确实" };
            foreach (string word in meaningfulWords)
            {
                if (line.Contains(word))
                    return true;
            }

            return false;
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
