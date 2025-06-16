using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.Data
{
    public class DatabaseInitializationService : IHostedService
    {
        private readonly TaskCompletionSource<bool> _initializationCompleted = new();

        // 提供一个Task让其他服务可以等待
        public Task InitializationTask => _initializationCompleted.Task;

        public DatabaseInitializationService()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("开始数据库初始化...");

                // 执行数据库初始化
                await DataService.InitializeAsync();
                await DataService.GetAppConfigAsync();

                Debug.WriteLine("DatabaseInitializationService数据库初始化完成。");

                // 标记初始化完成
                _initializationCompleted.SetResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"数据库初始化失败: {ex.Message}");
                _initializationCompleted.SetException(ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
