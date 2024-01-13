using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.ArenaInteractions
{
	public partial class UnitInteractionService : Node, IService
    {
        [Signal]
        public delegate void SelectedInteractionEventHandler(InteractionComponent interaction);

        [Signal]
        public delegate void ConfirmedInteractionEventHandler(InteractionComponent interaction);

        [Signal]
        public delegate void CanceledInteractionEventHandler();

        [Signal]
        public delegate void InvalidConfirmationEventHandler();

        [Inject]
        private FingerCursor _FingerCursor;

        // State vars
        private Vector3 _InitialPawnFacing;
        private LevelEntity _SelectedUnit;
        private Array<InteractionComponent> _Interactions = new Array<InteractionComponent>();
        private InteractionComponent _SelectedInteraction;

        public override void _Ready()
        {
            base._Ready();

            // Initially block processing
            SetProcess(false);
        }

        public void Activate(LevelEntity selectedUnit)
		{
            // Inject
            InjectionProvider.Inject(this);

            // Set the selected unit
            _SelectedUnit = selectedUnit;

            // Grab the initial unit facing dir
            _InitialPawnFacing = _SelectedUnit.FacingDirection;

            // Get all interactions in range
            // NB: This should always be more than 0 because otherwise we couldn't
            // have gotten here
            _Interactions = _SelectedUnit
                .GetComponent<UnitInteractionHandler>()
                .GetInteractionsInRange();

            // Select the first one
            SelectInteractable(_Interactions.First());

            // Allow processing
            SetProcess(true);
        }

        public void Deactivate()
        {
            SetProcess(false);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Confirm selected interactable
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // If the interaction is not available, emit invalid signal
                if (!_SelectedInteraction.Entity.IsActive())
                {
                    EmitSignal(nameof(InvalidConfirmation));
                    return;
                }

                // Emit a confirmation signal
                EmitSignal(nameof(ConfirmedInteraction), _SelectedInteraction);
                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // Otherwise, cancel signal
                EmitSignal(nameof(CanceledInteraction));

                // Reset unit facing
                _SelectedUnit.FacingDirection = _InitialPawnFacing;
                return;
            }

            // Rotate between selectables
            if (Input.IsActionJustPressed("move_left"))
            {
                // Get previous interactable in list
                var selectedIdx = _Interactions.IndexOf(_SelectedInteraction);
                var prevInteractable = _Interactions.SelectCircular(selectedIdx, -1);

                // If it's not the same as the previous interactable, select it
                if (prevInteractable != _SelectedInteraction) SelectInteractable(prevInteractable);
                return;
            }

            if (Input.IsActionJustPressed("move_right"))
            {
                // Get next interactable in list
                var selectedIdx = _Interactions.IndexOf(_SelectedInteraction);
                var nextInteractable = _Interactions.SelectCircular(selectedIdx, 1);

                // If it's not the same as the previous interactable, select it
                if (nextInteractable != _SelectedInteraction) SelectInteractable(nextInteractable);
                return;
            }
        }

        private void SelectInteractable(InteractionComponent interaction)
        {
            //    // Cancel the current diff
            //    CancelApDiff();

            //    // Hide the current interactable UI
            //    _InteractableUIContainer.Hide();
            // Set the interactable as the selected interaction
            _SelectedInteraction = interaction;
            //    // Activate its relevant UI
            //    _InteractableUIContainer.SelectInteractable(_SelectedInteractable);
            //    // Show it
            //    _InteractableUIContainer.Show();

            //    // If the selected unit can afford the interaction, show the confirmation UI
            //    var pawnCanAfford = SelectedUnitCanAffordInteraction(interactable);
            //    if (interactable.IsActive() && pawnCanAfford)
            //    {
            //        _InteractionConfirmationUI.SetCallToAction(interaction.CTA);
            //        _InteractionConfirmationUI.Show();
            //    }
            //    else _InteractionConfirmationUI.Hide();

            //    // If there are multiple interactables, show the cycle interactables UI
            //    if (_Interactables.Count > 1) _CycleInteractionsUI.Show();
            //    else _CycleInteractionsUI.Hide();

            // Point the finger cursor at the interaction
            var destination = _SelectedInteraction.Entity.GlobalPosition;
            _FingerCursor.PointCursorAt(destination, destination + (Vector3.Up * 2));

            // Face the pawn towards the interaction, if we're not on the same tile
            var closestTile = _SelectedInteraction.GetClosestInteractionTile(_SelectedUnit.GlobalPosition);
            if (closestTile.TilemapPosition != _SelectedUnit.TilemapPosition)
            {
                _SelectedUnit.FacingDirection = _SelectedUnit.GetFacingDirTowards(closestTile.GlobalPosition);
            }

            //    // Set the UI action cost diff
            //    RequestApDiff(new ActionPointHandlerComponent()
            //    {
            //        ActionPoints = interaction.InteractionAPCost
            //    });

            //    // Unhighlight other interactables and highlight this one
            //    foreach (var otherInteractable in _Interactables) otherInteractable.GetComponent<HoverEntityComponent>()?.Unhover();
            //    interactable.GetComponent<HoverEntityComponent>()?.Hover();
        }
    }
}

