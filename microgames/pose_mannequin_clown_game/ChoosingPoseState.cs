using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using System.Linq;

namespace ShopIsDone.Microgames.PoseMannequin
{
    public partial class ChoosingPoseState : State
    {
        [Signal]
        public delegate void ChangedPoseEventHandler();

        [Signal]
        public delegate void ConfirmedPoseEventHandler();

        [Signal]
        public delegate void RequestTimerEventHandler();

        [Export]
        private Node3D _PoseableMannequin;

        [Export]
        private AnimationPlayer _AnimPlayer;

        [Export]
        private Camera3D _Camera;

        [Export]
        private PoseManager _PoseManager;

        private Array<string> _CurrentPoses = new Array<string>();
        private int _CurrentIdx = 0;
        private SceneTreeTimer _Timer;

        public async override void OnStart(Dictionary<string, Variant> message)
        {
            // Set the next correct pose
            _PoseManager.PickNextPoseSet();

            // Pick set of poses for this try
            _CurrentIdx = 0;
            _CurrentPoses = _PoseManager.AvailablePoses;
            _CurrentPoses.Shuffle();
            // Make sure the correct pose isn't the first one the player sees
            while (_CurrentPoses.First() == _PoseManager.CurrentPose)
            {
                _CurrentPoses.Shuffle();
            }

            // Pick the first pose
            var initialPose = _CurrentPoses[_CurrentIdx];
            _AnimPlayer.Play(initialPose);

            // Swivel camera back
            var tween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(_Camera, "rotation_degrees:y", -180, 0.25f);
            await ToSignal(tween, "finished");

            // Finish on start hook
            base.OnStart(message);

            _Timer = GetTree().CreateTimer(5f, true);
            _Timer.Timeout += OnDelayFinished;
        }

        public void OnTimerFinished()
        {
            ChangeState(Consts.States.APPROACHING_MANNEQUIN, new Dictionary<string, Variant>()
            {
                { Consts.CHOSEN_POSE_KEY, _CurrentPoses[_CurrentIdx] }
            });
        }

        private void OnDelayFinished()
        {
            EmitSignal(nameof(RequestTimer));
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
                _Timer.Timeout -= OnDelayFinished;
                EmitSignal(nameof(ConfirmedPose));
                OnTimerFinished();
            }
        }

        private void ChangePose(int dir)
        {
            var nextPose = _CurrentPoses.SelectCircular(_CurrentIdx, Mathf.Sign(dir));
            _CurrentIdx = _CurrentPoses.IndexOf(nextPose);
            _AnimPlayer.Play(nextPose);
            EmitSignal(nameof(ChangedPose));

            // Juice the pose change
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