using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils;

namespace ShopIsDone.Tiles
{
	/* This class handles the Facing Direction of entities that need to be 
	 * positioned on an arena grid */
	[Tool]
	public partial class FacingDirectionHandler : NodeComponent
    {
        [Export]
        private bool _UpdateFacingTarget = true;

        [Signal]
        public delegate void FacingDirectionChangedEventHandler(Vector3 newDir);

		public Vector3 FacingDirection
		{
			get { return _FacingDirection; }
			set { OnFacingDirChanged(value); }
		}
		private Vector3 _FacingDirection = Vector3.Forward;

        [Export]
        public Node3D FacingTarget;

        public void SetFacingDirection(Vector3 dir)
        {
            FacingDirection = dir;
        }

        private void OnFacingDirChanged(Vector3 dir)
        {
            // Update the value
            _FacingDirection = dir;

            // Emit the signal
            EmitSignal(nameof(FacingDirectionChanged), dir);

            // If no facing target, return early
            if (FacingTarget == null || !_UpdateFacingTarget) return;

            // Otherwise, rotate the facing target when the facing direction changes
            FacingTarget.GlobalRotation = FacingTarget.GlobalRotation with
            {
                Y = Vec3.FacingDirToYRad(dir)
            };
        }
    }
}

