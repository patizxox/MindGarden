namespace MindGarden
{
    public record StageConfig(
    TimeSpan LightMinDelay,
    TimeSpan LightMaxDelay,
    int LightsPerStage,
    TimeSpan GrowMin,
    TimeSpan GrowMax
    );

}