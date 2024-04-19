using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Microgames.PoseMannequin
{
    public partial class ChoosingPoseState : State
    {
        [Signal]
        public delegate void ChangedPoseEventHandler();

        [Export]
        private Node3D _PoseableMannequin;

        [Export]
        private AnimationPlayer _AnimPlayer;

        private Array<string> _AllPoses = new Array<string>()
        {
            "FoldArms-Pose",
            "HandsOnHips-Pose",
            "Pray-Pose",
            "TurnRight-Pose",
            "Walk-Pose",
        };
        private Array<string> _CurrentPoses = new Array<string>();
        private int _CurrentIdx = 0;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Pick set of poses for this try
            _CurrentPoses = _AllPoses.Duplicate();
            _CurrentPoses.Shuffle();

            // Pick the first pose
            var initialPose = _CurrentPoses[_CurrentIdx];
            _AnimPlayer.Play(initialPose);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Handle pose switching input
            if (Input.IsActionJustPressed("move_right")) ChangePose(1);
            else if (Input.IsActionJustPressed("move_left")) ChangePose(-1);
            // Handle accept pose input
            else if (Input.IsActionJustPressed("ui_accept"))
            {

            }
        }

        private void ChangePose(int dir)
        {
            var nextPose = _CurrentPoses.SelectCircular(_CurrentIdx, Mathf.Sign(dir));
            _CurrentIdx = _CurrentPoses.IndexOf(nextPose);
            _AnimPlayer.Play(nextPose);
            EmitSignal(nameof(ChangedPose));
            var tween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(_PoseableMannequin, "scale", new Vector3(1.05f, 1.05f, 1.05f), 0.1f);
            tween.TweenProperty(_PoseableMannequin, "scale", Vector3.One, 0.05f);
        }
    }
}