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

