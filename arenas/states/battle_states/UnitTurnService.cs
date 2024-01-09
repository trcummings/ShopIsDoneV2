using System;
using Godot;
using System.Collections.Generic;
using ShopIsDone.Core;
using System.Linq;
using ShopIsDone.Actions;
using ShopIsDone.ActionPoints;
using static ShopIsDone.Actions.TeamHandler;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.ArenaInteractions;

namespace ShopIsDone.Arenas
{
    public partial class UnitTurnService : Node, IService
    {
        [Export]
        public Teams Team;

        public IEnumerable<LevelEntity> GetTurnEntities()
        {
            return GetTree()
                .GetNodesInGroup("entities")
                .OfType<LevelEntity>()
                // Filter by components
                .Where(e => e.HasComponent<TeamHandler>())
                // Make sure they're on our team
                .Where(e => e.GetComponent<TeamHandler>().Team == Team)
                // And they have the other necessary components
                .Where(e => e.HasComponent<ActionHandler>())
                .Where(e => e.HasComponent<ActionPointHandler>())
                //.Where(e => e.HasComponent<StatusEffectHandler>())
                // Make sure it's in the arena
                .Where(e => e.IsInArena())
                // Make sure it's enabled
                .Where(e => e.Enabled);
        }

        public void ResetActions()
        {
            var handlers = GetTurnEntities().Select(e => e.GetComponent<ActionHandler>());
            foreach (var handler in handlers) handler.ResetActions();
        }

        public IEnumerable<LevelEntity> GetActiveTurnEntities()
        {
            return GetTurnEntities()
                // Make sure they're alive
                .Where(e => !e.GetComponent<ActionPointHandler>().IsMaxedOut());
        }

        public void RefillApToMax()
        {
            var handlers = GetTurnEntities().Select(e => e.GetComponent<ActionPointHandler>());
            foreach (var handler in handlers) handler.RefillApToMax();
        }

        public void ResetInteractions()
        {
            var interacters = GetActiveTurnEntities().Where(e => e.HasComponent<UnitInteractionHandler>());
            foreach (var interacter in interacters)
            {
                var handler = interacter.GetComponent<UnitInteractionHandler>();
                handler.ResetInteractionState();
            }
        }

        //public Command ResolveStatusEffects(Arena arena)
        //{
        //    // Resolve status effects
        //    return new SeriesCommand(
        //        GetTurnEntities()
        //            // Filter out entities with no status effects
        //            .Where(e => e.HasComponent<StatusEffectHandlerComponent>())
        //            .Select(e => new SeriesCommand(
        //                e.GetComponent<StatusEffectHandlerComponent>().StatusEffects
        //                    .Select(effect => effect.ProcessStatusEffect(arena))
        //                    .ToArray()
        //            ))
        //            .ToArray()
        //    );
        //}
    }
}
