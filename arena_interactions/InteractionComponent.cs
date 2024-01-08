using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using System;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.ArenaInteractions
{
    public partial class InteractionComponent : Node3DComponent
    {
        // Strategy pattern for finishing interactions
        [Export]
        private InteractionFinishedHandler _InteractionFinishedHandler;

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
            // Init finished handler
            InjectionProvider.Inject(_InteractionFinishedHandler);
        }

        public virtual Command UpdateOnAction()
        {
            return new ActionCommand(SetInteractionInTiles);
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

        public virtual int GetInteractionApCost()
        {
            // Many interactions have no cost whatsoever
            return 0;
        }

        public const string UNIT_KEY = "Unit";
        public virtual Command RunInteraction(Dictionary<string, Variant> message = null)
        {
            // Finish the interaction
            return _InteractionFinishedHandler.FinishInteraction();
        }
    }
}
