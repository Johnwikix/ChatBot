using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Documents;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO; // 关键命名空间

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
                        return ReadPdfContent(filePath);
                    case ".docx":
                        return ReadDocxContent(filePath);
                    case ".txt":
                        return ReadTxtContent(filePath);
                    case ".md":
                        return ReadMarkdownContent(filePath);
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

        private static string ReadPdfContent(string filePath)
        {
            StringBuilder contentBuilder = new StringBuilder();

            try
            {
                // 这里使用iTextSharp库来读取PDF内容
                // 注意：需要安装iTextSharp包 (Install-Package iTextSharp)
                using (var reader = new iTextSharp.text.pdf.PdfReader(filePath))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        // 提取页面文本
                        string pageText = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(
                            reader, i, new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy());

                        // 转换编码以正确处理中文等字符
                        pageText = Encoding.UTF8.GetString(
                            Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(pageText)));

                        contentBuilder.AppendLine($"第 {i} 页:");
                        contentBuilder.AppendLine(pageText);
                    }
                }

                return contentBuilder.ToString();
            }
            catch (Exception ex)
            {
                // 如果读取失败，返回错误信息
                return $"无法读取PDF内容: {ex.Message}";
            }
        }

        private static string ReadTxtContent(string filePath)
        {
            try
            {
                // 尝试自动检测文件编码并读取文本
                using (var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                // 如果读取失败，返回错误信息
                return $"无法读取文本文件内容: {ex.Message}";
            }
        }

        private static string ReadMarkdownContent(string filePath)
        {
            try
            {
                // 与TXT文件类似，使用StreamReader读取MD文件
                using (var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                // 如果读取失败，返回错误信息
                return $"无法读取Markdown文件内容: {ex.Message}";
            }
        }
    }
    
}
