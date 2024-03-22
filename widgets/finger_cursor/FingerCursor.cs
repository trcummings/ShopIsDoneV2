using Godot;
using System;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Widgets
{
    public partial class FingerCursor : Node3D, IService
    {
        private readonly Vector3 _PointerOffset = Vector3.Up * 2.5F;

        // Nodes
        [Export]
        private MeshInstance3D _Pointer;

        public void WarpCursorTo(Vector3 globalPos)
        {
            // Set global translation
            GlobalPosition = globalPos;

            // Fix rotation
            GlobalRotation = Vector3.Zero;

            // Set pointer back to down
            _Pointer.GlobalPosition = globalPos + _PointerOffset;
            _Pointer.GlobalRotation = Vector3.Zero;
        }

        public void MoveCursorTo(Vector3 globalPos)
        {
            CreateTween()
                .BindNode(this)
                .TweenProperty(this, "global_position", globalPos, 0.2f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Quad);
        }

        public void PointCursorAt(Vector3 pointingAt, Vector3 pointerPos)
        {
            GlobalPosition = pointingAt;
            _Pointer.SafeLookAtFromPosition(pointingAt, pointerPos);
        }
    }
}