using Godot;
using ShopIsDone.Tiles;
using System;

namespace ShopIsDone.Levels
{
    // A DirectionalPoint imparts a position and a direction for units to use to
    // face that way when being moved or spawned in
    [Tool]
    public partial class DirectionalPoint : Node3D
    {
        [Signal]
        public delegate void FacingDirChangedEventHandler(Vector3 dir);

        #region Dir Tool Export
        [Export]
        private DirEnum.Dir EditorFacingDir
        {
            get { return DirEnum.VectorToDir(FacingDirection); }
            set { FacingDirection = DirEnum.DirToVector(value); }
        }
        #endregion

        [Export]
        public Vector3 FacingDirection
        {
            get { return _FacingDirection; }
            set
            {
                _FacingDirection = value;
                EmitSignal(nameof(FacingDirChanged), _FacingDirection);
            }
        }
        private Vector3 _FacingDirection = Vector3.Forward;

        private Node3D _Pivot;

        public override void _Ready()
        {
            base._Ready();
            _Pivot = GetNode<Node3D>("%Pivot");

            // If we're not in the editor, hide
            if (!Engine.IsEditorHint()) Hide();

            // Connect to event
            FacingDirChanged += (Vector3 _) => UpdateWidget();
            // Initially update
            UpdateWidget();
        }

        private void UpdateWidget()
        {
            _Pivot.LookAt(GlobalPosition + _FacingDirection);
        }
    }
}
