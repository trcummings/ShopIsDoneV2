using System;
using Godot;
using ShopIsDone.EntityStates;
using ShopIsDone.Models;

namespace ShopIsDone.Actors
{
	public partial class ActorAnimator : Node
	{
        [Signal]
        public delegate void FacingDirUpdatedEventHandler(Vector3 facingDir);

        [Export]
        private NodePath ModelPath;
        private IModel _Model;

        [Export]
        private EntityStateHandler _StateHandler;

        private const float NORM_VAL = 0.7071067811865475244f;
        private Vector3 _BackRightNorm = new Vector3(NORM_VAL, 0, NORM_VAL);
        private Vector3 _BackLeftNorm = new Vector3(NORM_VAL, 0, -NORM_VAL);
        private Vector3 _FrontRightNorm = new Vector3(-NORM_VAL, 0, NORM_VAL);
        private Vector3 _FrontLeftNorm = new Vector3(-NORM_VAL, 0, -NORM_VAL);

        public override void _Ready()
        {
            _Model = GetNode<IModel>(ModelPath);
        }

        public void Init()
        {
            _Model.Init();
        }

        public void UpdateAnimations(Vector3 velocity)
        {
            // Update animations based on velocity
            var isMoving = velocity.Length() > 0.001;
            if (isMoving && _StateHandler.IsInState("idle"))
            {
                _StateHandler.ChangeState("move");
            }
            else if (!isMoving && _StateHandler.IsInState("move"))
            {
                _StateHandler.ChangeState("idle");
            }

            // Update facing direction based on input
            var normalized = velocity.Normalized();
            var rounded = normalized.Round();
            var isOctalCase =
                Mathf.Abs(rounded.X) == 1 &&
                Mathf.Abs(rounded.X) == Mathf.Abs(rounded.Z);
            if (isOctalCase)
            {
                // Create a normed value based on the rounded vector
                var normed = rounded * NORM_VAL;

                // Update facing dir based on normed value
                if (normed == _BackRightNorm)
                {
                    EmitSignal(nameof(FacingDirUpdated), Vector3.Back);
                }
                else if (normed == _BackLeftNorm)
                {
                    EmitSignal(nameof(FacingDirUpdated), Vector3.Right);
                }
                else if (normed == _FrontLeftNorm)
                {
                    EmitSignal(nameof(FacingDirUpdated), Vector3.Forward);
                }
                else if (normed == _FrontRightNorm)
                {
                    EmitSignal(nameof(FacingDirUpdated), Vector3.Left);
                }
            }
            else if (rounded == Vector3.Zero)
            {
                // Do nothing
            }
            // Otherwise update
            else
            {
                EmitSignal(nameof(FacingDirUpdated), rounded);
            }
        }
    }
}

