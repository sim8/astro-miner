using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.Entities;

public class PlayerEntity(GameState gameState) : MiningControllableEntity(gameState)
{
    protected override float MaxSpeed => 4f;
    protected override float MaxHealth => GameConfig.PlayerMaxHealth;
    protected override int BoxSizePx { get; } = GameConfig.PlayerBoxSizePx;

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeMiningControls)
    {
        base.Update(gameTime, activeMiningControls);
        if (activeMiningControls.Contains(MiningControls.PlaceDynamite))
        {
            gameState.Ecs.DynamiteSystem.PlaceDynamite();
        }
    }

    public override Vector2 GetDirectionalLightSource()
    {
        return Direction switch
        {
            Direction.Top => Position + new Vector2(0.28f, -0.30f),
            Direction.Right => Position + new Vector2(0.32f, -0.30f),
            Direction.Bottom => Position + new Vector2(0.24f, -0.30f),
            Direction.Left => Position + new Vector2(0.26f, -0.28f),
            _ => Position
        };
    }
}