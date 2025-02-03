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
        if (activeMiningControls.Contains(MiningControls.PlaceDynamite) && gameState.Inventory.numDynamite > 0)
        {
            gameState.Inventory.numDynamite--;
            var entityId = gameState.DynamiteSystem.CreateDynamite(CenterPosition);
            var positionComponent = gameState.EcsWorld.GetComponent<PositionComponent>(entityId);
            positionComponent.Position = CenterPosition;
            positionComponent.SetPositionRelativeToDirectionalEntity(this, Direction.Top);
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