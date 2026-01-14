using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace MindGarden
{
    public class GameState
    {
        public GameStage Stage { get; set; }
        public int TotalPlants { get; set; }
        public double Multiplier { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        public List<int> UnlockedQuotes { get; set; } = new();

        public GameState() { }

        public GameState(GameStage stage, int totalPlants, double multiplier, int hits, int misses, List<int>? unlockedQuotes = null)
        {
            Stage = stage;
            TotalPlants = totalPlants;
            Multiplier = multiplier;
            Hits = hits;
            Misses = misses;
            UnlockedQuotes = unlockedQuotes ?? new List<int>();
        }
    }

    public static class SaveManager
    {
        private static readonly string SavePath = "savedata.json";

        public static void Save(GameState state)
        {
            try
            {
                var json = JsonSerializer.Serialize(state);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save: {ex}");
            }
        }

        public static GameState? Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return null;
                var json = File.ReadAllText(SavePath);
                return JsonSerializer.Deserialize<GameState>(json);
            }
            catch
            {
                return null;
            }
        }

        public static bool HasSave() => File.Exists(SavePath);
    }
}
