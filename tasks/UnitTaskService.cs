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
        public delegate void SelectedTaskEventHandler(TaskComponent task);

        [Signal]
        public delegate void ConfirmedTaskEventHandler(TaskComponent task);

        // UI signals
        [Signal]
        public delegate void SelectedEventHandler();

        [Signal]
        public delegate void ConfirmedEventHandler();

        [Signal]
        public delegate void CanceledEventHandler();

        [Signal]
        public delegate void InvalidEventHandler();

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

            // TODO: If there's more than one, show the "Cycle Tasks" prompt

            // Select the first one
            SelectTask(_Tasks.First());

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
                    EmitSignal(nameof(Invalid));
                    return;
                }

                // Emit a confirmation signal
                EmitSignal(nameof(Confirmed));
                EmitSignal(nameof(ConfirmedTask), _SelectedTask);
                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                Reset();
                // Otherwise, cancel signal
                EmitSignal(nameof(Canceled));
                return;
            }

            // Rotate between selectables
            if (Input.IsActionJustPressed("move_left"))
            {
                // Get previous task in list
                var selectedIdx = _Tasks.IndexOf(_SelectedTask);
                var prevTask = _Tasks.SelectCircular(selectedIdx, -1);

                // If it's not the same as the previous task, select it
                if (prevTask != _SelectedTask)
                {
                    SelectTask(prevTask);
                    EmitSignal(nameof(Selected));
                }
                return;
            }

            if (Input.IsActionJustPressed("move_right"))
            {
                // Get next task in list
                var selectedIdx = _Tasks.IndexOf(_SelectedTask);
                var nextTask = _Tasks.SelectCircular(selectedIdx, 1);

                // If it's not the same as the previous task, select it
                if (nextTask != _SelectedTask)
                {
                    SelectTask(nextTask);
                    EmitSignal(nameof(Selected));
                }

                return;
            }
        }

        private void SelectTask(TaskComponent task)
        {
            // Set the interactable as the selected interaction
            _SelectedTask = task;
            // Emit
            EmitSignal(nameof(SelectedTask), _SelectedTask);

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

            // Point the finger cursor at the task
            var destination = _SelectedTask.Entity.WidgetPoint.GlobalPosition;
            _FingerCursor.WarpCursorTo(destination);

            // Face the pawn towards the task, if we're not on the same tile
            var closestTile = _SelectedTask.GetClosestTaskTile(_SelectedUnit.GlobalPosition);
            if (closestTile.TilemapPosition != _SelectedUnit.TilemapPosition)
            {
                _SelectedUnit.FacingDirection = _SelectedUnit.GetFacingDirTowards(closestTile.GlobalPosition);
            }

            //    // Unhighlight other interactables and highlight this one
            //    foreach (var otherInteractable in _Interactables) otherInteractable.GetComponent<HoverEntityComponent>()?.Unhover();
            //    interactable.GetComponent<HoverEntityComponent>()?.Hover();
        }
    }
}

