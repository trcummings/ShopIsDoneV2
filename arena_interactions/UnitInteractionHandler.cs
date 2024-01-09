using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;
using Godot.Collections;
using ShopIsDone.ArenaInteractions.Selectors;

namespace ShopIsDone.ArenaInteractions
{
    // This component is used by an entity to interact with an interaction
    public partial class UnitInteractionHandler : NodeComponent
    {
        [Export]
        private Area3D _InteractionDetector;

        private Dictionary<string, Variant> _InteractionState = new Dictionary<string, Variant>();

        public void ResetInteractionState()
        {
            _InteractionState.Clear();
        }

        public void SetInteractionUsed(InteractionComponent interaction)
        {
            // Ignore if the interaction can be used multiple times per turn
            if (interaction.CanBeUsedMultipleTimesPerTurn) return;

            // Set the interaction in the state by the Id
            _InteractionState.Add(interaction.Entity.Id, true);
        }

        private bool WasInteractionUsed(InteractionComponent interaction)
        {
            return _InteractionState.ContainsKey(interaction.Entity.Id);
        }

        public bool HasAvailableInteractionsInRange()
        {
            return _InteractionDetector
                .GetOverlappingAreas()
                .OfType<ArenaInteractionSelectorTile>()
                .Select(tile => tile.Interaction)
                .Where(interaction => interaction.Entity.IsInArena())
                .Where(i => !WasInteractionUsed(i))
                .Count() > 0;
        }

        // Interaction
        public Array<InteractionComponent> GetInteractionsInRange()
        {
            return _InteractionDetector
                .GetOverlappingAreas()
                .OfType<ArenaInteractionSelectorTile>()
                .Select(tile => tile.Interaction)
                .Where(interaction => interaction.Entity.IsInArena())
                .ToGodotArray();
        }
    }
}

