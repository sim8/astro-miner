using System;
using System.Collections.Generic;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using AstroMiner.Utilities;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class HealthSystem : System
{
    public HealthSystem(World world, GameState gameState) : base(world, gameState)
    {
    }

    public void TakeDamage(int entityId, float damage)
    {
        var healthComponent = World.GetComponent<HealthComponent>(entityId);
        if (healthComponent == null || healthComponent.IsDead)
            return;

        healthComponent.CurrentHealth = Math.Max(0, healthComponent.CurrentHealth - damage);
        healthComponent.IsAnimatingDamage = true;
        healthComponent.TimeSinceLastDamageMs = 0;

        if (healthComponent.CurrentHealth == 0 && !healthComponent.IsDead)
        {
            healthComponent.IsDead = true;
            OnEntityDeath(entityId);
        }
    }

    private void OnEntityDeath(int entityId)
    {
        // Only create explosion if it's the miner dying
        if (entityId != World.MinerEntityId)
            return;

        // Create explosion at entity position
        var positionComponent = World.GetComponent<PositionComponent>(entityId);
        if (positionComponent != null)
            GameState.ExplosionSystem.CreateExplosion(positionComponent.CenterPosition);
    }

    public override void Update(GameTime gameTime, HashSet<MiningControls> activeControls)
    {
        // Update damage animation state
        foreach (var healthComponent in World.GetAllComponents<HealthComponent>())
        {
            if (healthComponent.IsAnimatingDamage)
            {
                // Update animation timers
                healthComponent.TotalDamageAnimationTimeMs += gameTime.ElapsedGameTime.Milliseconds;
                healthComponent.TimeSinceLastDamageMs += gameTime.ElapsedGameTime.Milliseconds;

                // Stop animation after time expires
                if (healthComponent.TimeSinceLastDamageMs >= GameConfig.DamageAnimationTimeMs)
                {
                    healthComponent.TotalDamageAnimationTimeMs = 0;
                    healthComponent.TimeSinceLastDamageMs = 0;
                    healthComponent.IsAnimatingDamage = false;
                }
            }
        }
    }
}