using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class PortalSystem : System
{
    public PortalSystem(Ecs ecs, GameState gameState) : base(ecs, gameState)
    {
    }

    private void MoveToTargetWorld(PortalConfig config, PositionComponent position)
    {
        position.World = config.TargetWorld;

        var targetCenter = new Vector2(config.Coordinates.Item1 + 0.5f, config.Coordinates.Item2 + 0.5f);

        targetCenter += config.Direction switch
        {
            Direction.Top => new Vector2(0, -1),
            Direction.Right => new Vector2(1, 0),
            Direction.Bottom => new Vector2(0, 1),
            Direction.Left => new Vector2(-1, 0),
            _ => new Vector2()
        };

        position.SetCenterPosition(targetCenter);

        // Only change active world if is ACE
        if (position.EntityId == GameState.Ecs.ActiveControllableEntityId)
            GameState.SetActiveWorldAndInitialize(config.TargetWorld);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        foreach (var movement in Ecs.GetAllComponents<MovementComponent>())
        {
            var entityId = movement.EntityId;
            var position = Ecs.GetComponent<PositionComponent>(entityId);
            Console.WriteLine("Pos: " + position.Position.Y + " centerPos: " + position.CenterPosition.Y);
            var direction = Ecs.GetComponent<DirectionComponent>(entityId);

            var (x, y) = ViewHelpers.ToGridPosition(position.CenterPosition);
            if (GameState.ActiveWorldState.CellIsPortal(x, y))
            {
                if (!movement.IsUsingPortal)
                {
                    movement.IsUsingPortal = true;
                    movement.CurrentSpeed = movement.MaxSpeed;
                }
                // If no transition in progress
                // set transition, cut off controls
                // mid transition - cut off everything?
                // callback to change world
                // 

                var config = WorldGrid.GetPortalConfig(position.World, (x, y));


                var distanceAtCurrentSpeed = movement.CurrentSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000f);

                if (config.Direction == Direction.Top)
                {
                    var distanceFromCenter = position.CenterPosition.X - (Math.Floor(position.CenterPosition.X) + 0.5f);

                    if (distanceFromCenter == 0)
                    {
                        direction.Direction = config.Direction;
                        var distanceToFarSide = position.CenterPosition.Y -
                                                (Math.Floor(position.CenterPosition.Y) + position.GridHeight / 2);
                        var distanceToTravel = Math.Min(distanceAtCurrentSpeed, distanceToFarSide);
                        position.Position +=
                            DirectionHelpers.GetDirectionalVector((float)distanceToTravel, direction.Direction);

                        // Has reached far end of portal cell
                        if (distanceToTravel == 0) MoveToTargetWorld(config, position);
                    }
                    else
                    {
                        direction.Direction = distanceFromCenter > 0 ? Direction.Left : Direction.Right;
                        var absDistanceToCenter = Math.Abs(distanceFromCenter);
                        var distanceToTravel = Math.Min(distanceAtCurrentSpeed, absDistanceToCenter);

                        position.Position +=
                            DirectionHelpers.GetDirectionalVector((float)distanceToTravel, direction.Direction);
                    }
                }

                // get AI to do this for left/right
            }
        }
    }
}