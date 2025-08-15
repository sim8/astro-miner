using System;
using AstroMiner.Definitions;
using AstroMiner.ECS.Components;
using Microsoft.Xna.Framework;

namespace AstroMiner.ECS.Systems;

public class HealthSystem : System
{
    public HealthSystem(Ecs ecs, BaseGame game) : base(ecs, game)
    {
    }

    public void KillAllEntitiesInWorld()
    {
        foreach (var healthComponent in Ecs.GetAllComponentsInActiveWorld<HealthComponent>())
            if (!healthComponent.IsDead)
            {
                healthComponent.CurrentHealth = 0;
                healthComponent.IsDead = true;
                OnEntityDeath(healthComponent.EntityId);
            }
    }

    public void TakeDamage(int entityId, float damage)
    {
        var healthComponent = Ecs.GetComponent<HealthComponent>(entityId);
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
        if (entityId != Ecs.MinerEntityId)
            return;

        // Create explosion at entity position
        var positionComponent = Ecs.GetComponent<PositionComponent>(entityId);
        if (positionComponent != null)
            game.StateManager.Ecs.Factories.CreateExplosionEntity(positionComponent.CenterPosition);
    }

    public override void Update(GameTime gameTime, ActiveControls activeControls)
    {
        if (game.Model.ActiveWorld != World.Asteroid) return;

        // Update damage animation state
        foreach (var healthComponent in Ecs.GetAllComponents<HealthComponent>())
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