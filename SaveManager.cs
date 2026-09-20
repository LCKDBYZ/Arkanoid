using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Arkanoid {
    internal class SaveManager {
        private static string GetSavePath() {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ArkanoidGame"
            );
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "savedata.json");
        }

        public static SaveData Load() {
            string path = GetSavePath();
            if (!File.Exists(path)) {
                return new SaveData(); // default
            }

            try {
                string json = File.ReadAllText(path);
                SaveData data = JsonSerializer.Deserialize<SaveData>(json);
                return data ?? new SaveData();
            }
            catch {
                return new SaveData(); // if file is broken, it returns default
            }
        }

        public static void Save(SaveData data) {
            string path = GetSavePath();
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
