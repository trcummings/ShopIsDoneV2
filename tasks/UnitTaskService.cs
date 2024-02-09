using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Tasks
{
    public partial class UnitTaskService : Node, IService
    {
        [Signal]
        public delegate void SelectedInteractionEventHandler(TaskComponent task);

        [Signal]
        public delegate void ConfirmedInteractionEventHandler(TaskComponent task);

        [Signal]
        public delegate void CanceledInteractionEventHandler();

        [Signal]
        public delegate void InvalidConfirmationEventHandler();

        [Inject]
        private FingerCursor _FingerCursor;

        // State vars
        private Vector3 _InitialPawnFacing;
        private LevelEntity _SelectedUnit;
        private Array<TaskComponent> _Tasks = new Array<TaskComponent>();
        private TaskComponent _SelectedTask;

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
            _Tasks = _SelectedUnit
                .GetComponent<TaskHandler>()
                .GetTasksInRange();

            // Select the first one
            SelectInteractable(_Tasks.First());

            // Allow processing
            SetProcess(true);
        }

        public void Deactivate()
        {
            SetProcess(false);
        }

        public void Reset()
        {
            // Reset unit facing
            _SelectedUnit.FacingDirection = _InitialPawnFacing;
            // Reset finger cursor position
            _FingerCursor.WarpCursorTo(_SelectedUnit.GlobalPosition);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Confirm selected interactable
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // If the interaction is not available, emit invalid signal
                if (!_SelectedTask.Entity.IsActive())
                {
                    EmitSignal(nameof(InvalidConfirmation));
                    return;
                }

                // Emit a confirmation signal
                EmitSignal(nameof(ConfirmedInteraction), _SelectedTask);
                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                Reset();
                // Otherwise, cancel signal
                EmitSignal(nameof(CanceledInteraction));
                return;
            }

            // Rotate between selectables
            if (Input.IsActionJustPressed("move_left"))
            {
                // Get previous interactable in list
                var selectedIdx = _Tasks.IndexOf(_SelectedTask);
                var prevInteractable = _Tasks.SelectCircular(selectedIdx, -1);

                // If it's not the same as the previous interactable, select it
                if (prevInteractable != _SelectedTask) SelectInteractable(prevInteractable);
                return;
            }

            if (Input.IsActionJustPressed("move_right"))
            {
                // Get next interactable in list
                var selectedIdx = _Tasks.IndexOf(_SelectedTask);
                var nextInteractable = _Tasks.SelectCircular(selectedIdx, 1);

                // If it's not the same as the previous interactable, select it
                if (nextInteractable != _SelectedTask) SelectInteractable(nextInteractable);
                return;
            }
        }

        private void SelectInteractable(TaskComponent task)
        {
            //    // Cancel the current diff
            //    CancelApDiff();

            //    // Hide the current interactable UI
            //    _InteractableUIContainer.Hide();
            // Set the interactable as the selected interaction
            _SelectedTask = task;
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
            var destination = _SelectedTask.Entity.GlobalPosition;
            _FingerCursor.PointCursorAt(destination, destination + (Vector3.Up * 2));

            // Face the pawn towards the task, if we're not on the same tile
            var closestTile = _SelectedTask.GetClosestTaskTile(_SelectedUnit.GlobalPosition);
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

