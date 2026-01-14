namespace MindGarden
{
    public static class GamePresets
    {
        public static Dictionary<GameStage, StageConfig> GetPresets()
        {
            var stages = new Dictionary<GameStage, StageConfig>
            {
                [GameStage.Stage1] = new(TimeSpan.FromSeconds(1.2), TimeSpan.FromSeconds(2.5), 5, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)),
                [GameStage.Stage2] = new(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3.5), 7, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(7)),
                [GameStage.Stage3] = new(TimeSpan.FromSeconds(1.8), TimeSpan.FromSeconds(4.0), 8, TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(9)),
                [GameStage.Stage4] = new(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(4.5), 10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)),
                [GameStage.Stage5] = new(TimeSpan.FromSeconds(2.2), TimeSpan.FromSeconds(5.0), 10, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(12)),
            };

            return stages;
        }
    }

}