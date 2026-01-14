using System.IO;
using System.Text.Json;

namespace MindGarden
{
    public record GameState(
        GameStage Stage,
        int TotalPlants,
        double Multiplier,
        int Hits,
        int Misses
    );

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
