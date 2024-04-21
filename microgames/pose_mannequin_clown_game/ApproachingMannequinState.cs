using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using System.Linq;

namespace ShopIsDone.Microgames.PoseMannequin
{
    public partial class ApproachingMannequinState : State
    {
        [Signal]
        public delegate void SelectedCorrectPoseEventHandler();

        [Signal]
        public delegate void FailedSelectionEventHandler();

        [Signal]
        public delegate void FailedForLastTimeEventHandler();

        [Signal]
        public delegate void LightBuzzEventHandler();

        [Export]
        private ShaderMaterial _DistortionMaterial;

        [Export]
        private Camera3D _Camera;

        [Export]
        private Node3D _Mannequin;

        [Export]
        private Node3D _PositionsParent;
        private Array<Marker3D> _PositionMarkers = new Array<Marker3D>();
        private Marker3D _CurrentMarker;

        [Export]
        private AnimationPlayer _AnimPlayer;

        [Export]
        private AnimationPlayer _FinalAnimPlayer;

        [Export]
        private PoseManager _PoseManager;

        [Export]
        private float _ViewMannequinTime = 5f;
        private int _Intensity = 0;

        public override void _Ready()
        {
            base._Ready();
            _PositionMarkers = _PositionsParent
                .GetChildren()
                .OfType<Marker3D>()
                .ToGodotArray();
        }

        public async override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
            var isFirstTime = (bool)message.GetValueOrDefault(Consts.IS_FIRST_TIME_KEY, false);

            // If it's the first time, just set the camera without turning it
            if (isFirstTime)
            {
                BumpIntensity();
                // Pick mannequin pose
                _PoseManager.Init();
                // Pose mannequin
                _AnimPlayer.Play(_PoseManager.CurrentPose);
                // Set next marker
                _CurrentMarker = _PositionMarkers.First();
                _PositionMarkers.Remove(_CurrentMarker);
                // Move mannequin to marker
                _Mannequin.Transform = _CurrentMarker.Transform;
                // Force camera to perspective
                _Camera.RotationDegrees = _Camera.RotationDegrees with { Y = 0 };
                // Wait a tick
                await ToSignal(GetTree().CreateTimer(_ViewMannequinTime, true), "timeout");
                // Then change to the pose state
                ChangeState(Consts.States.CHOOSING_POSE);
            }
            // Otherwise, tween over there
            else
            {
                // Move to next marker
                _CurrentMarker = _PositionMarkers.First();
                _PositionMarkers.Remove(_CurrentMarker);

                // Pull selected pose from message
                var chosenPose = (string)message.GetValueOrDefault(Consts.CHOSEN_POSE_KEY, "");

                // Move mannequin to marker
                _Mannequin.Transform = _CurrentMarker.Transform;
                // Pose mannequin
                _AnimPlayer.Play(_PoseManager.CurrentPose);

                // Swivel camera back
                var tween = GetTree()
                    .CreateTween()
                    .BindNode(this)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.Out);
                tween.TweenProperty(_Camera, "rotation_degrees:y", 0, 0.25f);
                await ToSignal(tween, "finished");

                // Compare the selected pose to the current pose. If they're the
                // same, end the microgame
                if (_PoseManager.CurrentPose == chosenPose)
                {
                    await ToSignal(GetTree().CreateTimer(2f), "timeout");

                    var idx = _CurrentMarker.GetIndex();
                    var groupName = $"pmcm_spot_{idx + 1}";
                    var lights = GetTree().GetNodesInGroup(groupName).OfType<Light3D>();

                    // Kill the lights at that spot
                    foreach (var light in lights) light.Hide();
                    EmitSignal(nameof(LightBuzz));

                    // Hide mannequin
                    _Mannequin.Hide();
                    // Wait a little
                    await ToSignal(GetTree().CreateTimer(1f), "timeout");

                    // Turn them back on
                    foreach (var light in lights) light.Show();

                    // Wait a little
                    await ToSignal(GetTree().CreateTimer(1f), "timeout");

                    EmitSignal(nameof(SelectedCorrectPose));
                }
                // If they're wrong and it's not the last marker, then emit an
                // incorrect signal
                else if (_PositionMarkers.Count > 0)
                {
                    BumpIntensity();

                    // Narrow the available set on the second one
                    if (_PositionMarkers.Count == 2) _PoseManager.NarrowSet();

                    await ToSignal(GetTree().CreateTimer(1, true), "timeout");

                    EmitSignal(nameof(FailedSelection));

                    // Wait a tick
                    await ToSignal(GetTree().CreateTimer(_ViewMannequinTime - 1, true), "timeout");

                    // Go back to choosing pose state
                    ChangeState(Consts.States.CHOOSING_POSE);
                }
                // If we're on the last one
                else
                {
                    await ToSignal(GetTree().CreateTimer(2, true), "timeout");

                    EmitSignal(nameof(FailedSelection));

                    // Cut the lights
                    var lights = GetTree().GetNodesInGroup("pmcm_lights").OfType<Light3D>();
                    // Kill the lights at that spot
                    foreach (var light in lights) light.Hide();
                    EmitSignal(nameof(LightBuzz));

                    // Have the mannequin pray
                    _AnimPlayer.Play("Desperation-Pose-1");
                    _Mannequin.RotationDegrees = _Mannequin.RotationDegrees with { Y = -180 };

                    await ToSignal(GetTree().CreateTimer(2, true), "timeout");

                    // Turn on the flashlight
                    var flashlight = lights.Where(l => l.IsInGroup("pmcm_flashlight")).First();
                    flashlight.Show();

                    await ToSignal(GetTree().CreateTimer(2, true), "timeout");

                    _FinalAnimPlayer.Play("default");
                    await ToSignal(_FinalAnimPlayer, "animation_finished");

                    EmitSignal(nameof(FailedForLastTime));
                }
            }
        }

        public void BumpIntensity()
        {
            _Intensity += 1;
            // Up the distortion amount
            _DistortionMaterial.SetShaderParameter("intensity", .3 + _Intensity * .1);
            // Play a random lighting effect
        }
    }
}

