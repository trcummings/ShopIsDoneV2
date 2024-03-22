using Godot;
using System;

namespace ShopIsDone.BreakRoom
{
    public partial class AutoDoor : Area3D
    {
        [Signal]
        public delegate void DoorOpenedEventHandler();

        [Signal]
        public delegate void DoorClosedEventHandler();

        [Export]
        private Node3D _Door;

        private Tween _DoorTween;

        public override void _Ready()
        {
            BodyEntered += (Node3D _) => OpenDoor();
            BodyExited += (Node3D _) => CloseDoor();
        }

        private void OpenDoor()
        {
            if (_DoorTween != null)
            {
                _DoorTween.Kill();
                _DoorTween = null;
            }

            _DoorTween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Back);
            _DoorTween.TweenProperty(_Door, "rotation:y", Mathf.DegToRad(140), 0.4f);

            EmitSignal(nameof(DoorOpened));
        }

        private void CloseDoor()
        {
            if (_DoorTween != null)
            {
                _DoorTween.Kill();
                _DoorTween = null;
            }

            _DoorTween = GetTree()
                .CreateTween()
                .SetEase(Tween.EaseType.InOut)
                .SetTrans(Tween.TransitionType.Back);
            _DoorTween.TweenProperty(_Door, "rotation:y", 0, 0.65f);

            // Oneshot connect for when the door closes
            _DoorTween.Connect(
                "finished",
                Callable.From(() => EmitSignal(nameof(DoorClosed))),
                (uint)ConnectFlags.OneShot
            );
        }
    }
}

