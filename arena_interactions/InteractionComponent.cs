using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using System;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Tiles;
using ShopIsDone.ArenaInteractions.Selectors;

namespace ShopIsDone.ArenaInteractions
{
    public partial class InteractionComponent : Node3DComponent
    {
        [Export]
        public bool CanBeUsedMultipleTimesPerTurn = false;

        [Export]
        public bool EndsTurnAfterInteraction = false;

        // Strategy pattern for finishing interactions
        [Export]
        private InteractionFinishedHandler _InteractionFinishedHandler;

        [Export]
        private ArenaInteractionSelector _InteractionSelector;

        // Tiles that that an interactor can stand and interact with the interaction on
        protected Array<InteractionTile> _InteractionTiles = new Array<InteractionTile>();

        public override void _Ready()
        {
            base._Ready();

            // Ready interaction tiles
            _InteractionTiles = GetChildren().OfType<InteractionTile>().ToGodotArray();
        }

        public override void Init()
        {
            base.Init();
            // Register tiles with interactables and the associated tile
            SetInteractionInTiles();
            // Init interaction selector
            _InteractionSelector.Init(this);
            // Init finished handler
            InjectionProvider.Inject(_InteractionFinishedHandler);
        }

        public Tile GetClosestSelectableTile(Vector3 pos)
        {
            return _InteractionTiles
                .OrderBy(iTile => iTile.GlobalPosition.DistanceTo(pos))
                .Select(iTile => iTile.Tile)
                .FirstOrDefault();
        }

        private void SetInteractionInTiles()
        {
            // Register tiles with interaction and the associated tile
            foreach (var iTile in _InteractionTiles)
            {
                iTile.Interaction = this;
            }
        }

        public Array<InteractionTile> GetInteractionTiles()
        {
            return _InteractionTiles;
        }

        public virtual Command RunInteraction(
            UnitInteractionHandler handler,
            Dictionary<string, Variant> message = null
        )
        {
            // Finish the interaction
            return _InteractionFinishedHandler.FinishInteraction();
        }
    }
}
