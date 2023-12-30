using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using Godot.Collections;
using ShopIsDone.Actions;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
	public partial class FacingDirectionState : ActionState
    {
        [Signal]
        public delegate void ChangedDirectionEventHandler();

        [Signal]
        public delegate void ConfirmedDirectionEventHandler();

        [Signal]
        public delegate void CanceledSelectionEventHandler();

        // Nodes
        [Inject]
        private FacingWidget _FacingWidget;

        [Inject]
        private DirectionalInputHelper _DirectionalInputHelper;

        // State Variables
        private Vector3 _InitialUnitFacingDir = Vector3.Zero;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            InjectionProvider.Inject(this);

            // Persist its initial facing direction
            _InitialUnitFacingDir = _SelectedUnit.FacingDirection;

            // Initialize the widget
            _FacingWidget.WarpToLocation(_SelectedUnit.GlobalPosition);
            _FacingWidget.SelectFacingDir(_SelectedUnit.FacingDirection);
            _FacingWidget.Show();
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Confirm facing direction
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // Emit confirmation signal
                EmitSignal(nameof(ConfirmedDirection));

                // Run the action
                EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
                {
                    { Consts.ACTION_KEY, _Action },
                });
                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // Emit Cancellation Signal
                EmitSignal(nameof(CanceledSelection));

                // If we've chosen a direction, revert that direction
                if (_SelectedUnit.FacingDirection != _InitialUnitFacingDir)
                {
                    SetFacingDir(_InitialUnitFacingDir);
                }
                // Otherwise, if our chosen direction is the same as the initial
                // direction, request to cancel the current state instead
                else EmitSignal(nameof(MainMenuRequested));

                return;
            }

            // Get transformed movement vector from input
            var moveVec = _DirectionalInputHelper.InputDir;

            // Ignore if no movement input
            if (moveVec == Vector3.Zero) return;

            // Otherwise, set the facing direction
            SetFacingDir(moveVec);

            // And Emit a signal that the direction changed
            EmitSignal(nameof(ChangedDirection));
        }

        public override void OnExit(string nextState)
        {
            base.OnExit(nextState);

            // Hide the facing widget
            _FacingWidget.Hide();

            // Reset state vars
            _InitialUnitFacingDir = Vector3.Zero;
        }

        private void SetFacingDir(Vector3 dir)
        {
            // Set the facing direction of the widget
            _FacingWidget.SelectFacingDir(dir);

            // Set the unit facing direction
            _SelectedUnit.FacingDirection = dir;
        }
    }
}
