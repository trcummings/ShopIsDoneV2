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
        [Signal]
        public delegate void FacingDirectionChangedEventHandler(Vector3 newDir);

        #region Dir Tool Export
        public enum DirEnum
        {
            Forward = 0,
            Back = 1,
            Left = 2,
            Right = 3
        }
        [Export]
		private DirEnum EditorFacingDir
        {
            get {
                // Compare against the current facing direction
                if (_FacingDirection == Vector3.Forward) return DirEnum.Forward;
                if (_FacingDirection == Vector3.Back) return DirEnum.Back;
                if (_FacingDirection == Vector3.Left) return DirEnum.Left;
                if (_FacingDirection == Vector3.Right) return DirEnum.Right;
                return DirEnum.Forward;
            }
            set
            {
                // Set facing direction based on the dir enum
                switch (value)
                {
                    case DirEnum.Forward:
                        {
                            FacingDirection = Vector3.Forward;
                            break;
                        }
                    case DirEnum.Back:
                        {
                            FacingDirection = Vector3.Back;
                            break;
                        }
                    case DirEnum.Left:
                        {
                            FacingDirection = Vector3.Left;
                            break;
                        }
                    case DirEnum.Right:
                        {
                            FacingDirection = Vector3.Right;
                            break;
                        }
                }
            }
        }
        #endregion

		public Vector3 FacingDirection
		{
			get { return _FacingDirection; }
			set { _FacingDirection = value; EmitSignal(nameof(FacingDirectionChanged), value); }
		}
		private Vector3 _FacingDirection = Vector3.Forward;

        [Export]
        public Node3D FacingTarget;

        public override void _Ready()
        {
            base._Ready();
            FacingDirectionChanged += OnFacingDirChanged;
        }

        public void SetFacingDirection(Vector3 dir)
        {
            FacingDirection = dir;
        }

        private void OnFacingDirChanged(Vector3 newDir)
        {
            // Rotate the facing target when the facing direction changes
            if (FacingTarget == null) return;
            FacingTarget.GlobalRotation = FacingTarget.GlobalRotation with
            {
                Y = Vec3.FacingDirToYRad(newDir)
            };
        }
    }
}

