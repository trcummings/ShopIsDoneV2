using System;
using Godot;

namespace ShopIsDone.Microgames.HallwayChase
{
	public partial class HallwayChasePlayer : CharacterBody3D
	{
        [Export]
        public float MouseSensitivity = 2.0f;

        private MovementController _MovementController;
        private HeadController _HeadController;
        private SprintController _SprintController;
        private FootstepController _FootstepController;

        // Input vars
        private Vector2 _LookInput;
        private Vector2 _MoveInput;
        private float _MouseSensitivity;

        public override void _Ready()
        {
            base._Ready();
            _MovementController = GetNode<MovementController>("%MovementController");
            _HeadController = GetNode<HeadController>("%HeadController");
            _SprintController = GetNode<SprintController>("%SprintController");
            _FootstepController = GetNode<FootstepController>("%FootstepController");

            // Set mouse sensitivity
            _MouseSensitivity = MouseSensitivity / 1000;
        }

        public override void _Input(InputEvent @event)
        {
            // Get mouse look input
            if (@event is InputEventMouseMotion mouseEvent && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                _LookInput = mouseEvent.Relative * _MouseSensitivity;
            }
        }

        public override void _Process(double delta)
        {
            // Get joystick look input
            var joystickAxis = Input.GetVector(
                "fps_look_left",
                "fps_look_right",
                "fps_look_up",
                "fps_look_down"
            );
            if (joystickAxis != Vector2.Zero)
            {
                _LookInput = joystickAxis * 1000f * (float)delta * _MouseSensitivity;
            }

            // Get move input
            _MoveInput = Input.GetVector(
                "fps_move_backward",
                "fps_move_forward",
                "fps_move_left",
                "fps_move_right"
            );
        }

        public override void _PhysicsProcess(double delta)
        {
            // Handle sprint
            var canSprint = IsOnFloor() && Input.IsActionPressed("fps_sprint") && _MoveInput.X >= 0.5;
            _SprintController.Sprint(canSprint, (float)delta);

            // Move
            _MovementController.Move(_MoveInput, (float)delta);

            // Calculate current percent of max speed
            var percentOfMaxSpeed = Mathf.Max(
                Mathf.Abs(_MovementController.Velocity.X),
                Mathf.Abs(_MovementController.Velocity.Z)
            ) / _SprintController.SprintSpeed;

            // Look
            _HeadController.Look(_LookInput, Mathf.Sign(_MovementController.Velocity.Z) * percentOfMaxSpeed, (float)delta);

            // Get is moving
            var isMoving = _MovementController.IsMoving();

            // Apply head bob
            _HeadController.ApplyHeadBob(isMoving, percentOfMaxSpeed, (float)delta);

            // Progress footfalls by calculating foot speed
            var footSpeed = FootstepController.FootSpeed.Slow;
            if (Mathf.Abs(_MoveInput.X) >= 0.5f || Mathf.Abs(_MoveInput.Y) >= 0.5f)
            {
                footSpeed = FootstepController.FootSpeed.Medium;
            }
            if (canSprint && _SprintController.IsSprinting())
            {
                footSpeed = FootstepController.FootSpeed.Fast;
            }
            _FootstepController.ProcessFootsteps(isMoving && IsOnFloor(), footSpeed, (float)delta);

            // Clear input
            _MoveInput = Vector2.Zero;
            _LookInput = Vector2.Zero;
        }
    }
}

