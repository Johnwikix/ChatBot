using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wpfChat.Models;

namespace wpfChat.Data
{
    public class DataService
    {
        private static SQLiteAsyncConnection _dbConnection;
        private static readonly string DbPath = "Database.db";
        public static async Task InitializeAsync()
        {
            if (_dbConnection == null)
            {
                _dbConnection = new SQLiteAsyncConnection(DbPath);
                await _dbConnection.CreateTableAsync<SaveConfig>();
            }
            Debug.WriteLine("数据库连接已初始化。");
        }

        public static async Task GetAppConfigAsync()
        {
            var config = await _dbConnection.Table<SaveConfig>().FirstOrDefaultAsync();
            if (config != null) {
                AppConfig.ModelFolder = config.ModelFolder;    
                AppConfig.ModelPath = config.ModelPath;
            }            
        }

        public static async Task<string> GetModePath()
        {
            var config = await _dbConnection.Table<SaveConfig>().FirstOrDefaultAsync();
            if (config != null)
            {
                return config.ModelFolder;
            }
            return string.Empty;
        }
        public static async Task SaveAppConfigAsync(SaveConfig config)
        {
            var existingConfig = await _dbConnection.Table<SaveConfig>().FirstOrDefaultAsync();
            if (existingConfig != null)
            {
                config.Id = existingConfig.Id; // 保持 ID 不变
                await _dbConnection.UpdateAsync(config);
            }
            else
            {
                await _dbConnection.InsertAsync(config);
            }
        }
    }
}
