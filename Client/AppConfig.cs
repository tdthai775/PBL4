using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace Client
{
    class AppConfig
    {
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 8080;

        private static string configPath = "config.json";

        public static AppConfig Load()
        {
            if (!File.Exists(configPath))
            {
                var defaultConfig = new AppConfig();
                File.WriteAllText(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
                return defaultConfig;
            }

            string json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }
    }
}
