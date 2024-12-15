namespace AstroMiner;

public class UserInterfaceState(GameState gameState)
{
    private const int ZoomTransitionMsPerZoomLevel = 1000;

    // ScaleMultiplier has slow transitions - BaseScaleMultiplier represents the "destination" value
    private float BaseScaleMultiplier => gameState.IsInMiner ? GameConfig.ZoomLevelMiner : GameConfig.ZoomLevelPlayer;
    public float ScaleMultiplier { get; set; } = GameConfig.ZoomLevelPlayer;

    public void Update(int elapsedMs)
    {
        // TODO make this smoother? Or at least constant
        var ZoomLevelDiff = BaseScaleMultiplier - ScaleMultiplier;
        var percentageComplete = elapsedMs / (float)ZoomTransitionMsPerZoomLevel;
        ScaleMultiplier += ZoomLevelDiff * percentageComplete;
    }
}