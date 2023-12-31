﻿using System;
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
        [Export]
		private DirEnum.Dir EditorFacingDir
        {
            get { return DirEnum.VectorToDir(_FacingDirection); }
            set { FacingDirection = DirEnum.DirToVector(value); }
        }
        #endregion

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

            // Rotate the facing target when the facing direction changes
            if (FacingTarget == null) return;
            FacingTarget.GlobalRotation = FacingTarget.GlobalRotation with
            {
                Y = Vec3.FacingDirToYRad(dir)
            };

            // Emit the signal
            EmitSignal(nameof(FacingDirectionChanged), dir);
        }
    }
}

