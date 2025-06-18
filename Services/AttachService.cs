using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Documents;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing; // 关键命名空间

namespace wpfChat.Services
{
    public class AttachService
    {
        public static string GetAttach(string filePath) {
            // 检查文件是否存在
            if (System.IO.File.Exists(filePath))
            {
                // 获取文件名和扩展名
                string fileName = System.IO.Path.GetFileName(filePath);
                string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();                
                switch (fileExtension)
                {
                    case ".pdf":
                        return $"PDF文件：{fileName}";
                    case ".docx":
                        return ReadDocxContent(filePath);
                    case ".txt":
                        return $"文本文件：{fileName}";
                    case ".md":
                        return $"Markdown文件：{fileName}";
                    default:
                        return $"未知文件类型：{fileName}";
                }
            }
            else
            {
                return "文件不存在";
            }
        }

        private static string ReadDocxContent(string filePath)
        {
            StringBuilder contentBuilder = new StringBuilder();

            // 使用using语句确保资源正确释放
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                // 获取主文档部分
                MainDocumentPart mainPart = doc.MainDocumentPart;
                if (mainPart != null && mainPart.Document != null)
                {
                    // 明确指定使用DocumentFormat.OpenXml.Wordprocessing.Paragraph
                    var paragraphs = mainPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();

                    // 遍历每个段落提取文本
                    foreach (var paragraph in paragraphs)
                    {
                        // 提取段落中的所有文本
                        string paragraphText = string.Join(string.Empty,
                            paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>()
                                .SelectMany(run => run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>())
                                .Select(text => text.Text ?? string.Empty));

                        // 添加段落文本，用换行符分隔
                        contentBuilder.AppendLine(paragraphText);
                    }
                }
            }

            return contentBuilder.ToString();
        }
    }
    
}
