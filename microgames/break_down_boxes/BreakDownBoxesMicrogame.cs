using Godot;
using System;
using ShopIsDone.Cameras;
using Godot.Collections;
using ShopIsDone.Utils;
using System.Linq;

namespace ShopIsDone.Microgames.BreakDownBoxes
{
    public partial class BreakDownBoxesMicrogame : Microgame
    {
        [Export]
        private PackedScene TapeManagerScene;

        [Export]
        private AnimationPlayer _BoxAnimPlayer;

        [Signal]
        public delegate void MicrosleepEventHandler();

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private CharacterBody2D _BoxCutter;
        private Camera3D _Camera3D;
        private Node2D _TapePoints;

        // Speed and acceleration
        private float _MaxSpeed = 800;
        private float _Acceleration = 1200;
        private float _Friction = 0.95f;

        public const string TAPE_GROUP = "bdb_tape";
        public const string TOP_ALIGNED_GROUP = "bdb_top_aligned";
        public const string BACK_ALIGNED_GROUP = "bdb_back_aligned";
        public const string FRONT_ALIGNED_GROUP = "bdb_front_aligned";
        private Array<string> _TapeGroups = new Array<string>()
        {
            TOP_ALIGNED_GROUP,
            BACK_ALIGNED_GROUP,
            FRONT_ALIGNED_GROUP
        };
        private Array<TapeManager> _Tapes = new Array<TapeManager>();

        private Dictionary<string, Vector3> _Orientations = new Dictionary<string, Vector3>()
        {
            { "top_up_left", Vec3.Zero },
            { "top_up_right", Vec3.Up },
            { "left_horizontal", Vec3.Back },
            { "left_vertical", Vec3.BackRight },
            { "right_horizontal", Vec3.UpBack },
            { "right_vertical", Vec3.UpBackLeft }
        };

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _BoxCutter = GetNode<CharacterBody2D>("%BoxCutter");
            _Camera3D = GetNode<Camera3D>("%Camera3D");
            _TapePoints = GetNode<Node2D>("%TapePoints");

            // Connect screenshake
            _Screenshake.ShakeOffsetUpdated += ShakeUpdate;

            // Do not allow player input
            SetProcess(false);
            SetPhysicsProcess(false);
        }

        public override void Init(Dictionary<string, Variant> msg)
        {
            base.Init(msg);

            // Pick orientation

            // Collect tape
            foreach (var tapeGroup in _TapeGroups)
            {
                var manager = TapeManagerScene.Instantiate<TapeManager>();
                AddChild(manager);
                manager.Init(tapeGroup);
                manager.GenerateCutPoints(3, _Camera3D, _TapePoints);
                _Tapes.Add(manager);
                // Connect
                manager.AllPointsCut += OnTapeCut;
            }

        }

        public override void Start()
        {
            base.Start();

            // Allow player input
            SetProcess(true);
            SetPhysicsProcess(true);
        }

        public override void _PhysicsProcess(double delta)
        {
            // Move reticule
            var moveDir = new Vector2(
                Input.GetAxis("fps_move_left", "fps_move_right"),
                Input.GetAxis("fps_move_backward", "fps_move_forward")
            ).Normalized();

            // Flip move dir in y direction
            moveDir.Y = -moveDir.Y;

            // Update velocity
            if (moveDir == Vector2.Zero)
            {
                _BoxCutter.Velocity = Vector2.Zero;
            }
            else
            {
                _BoxCutter.Velocity = new Vector2(
                    CalculateVelocity(moveDir.X, _BoxCutter.Velocity.X, (float)delta),
                    CalculateVelocity(moveDir.Y, _BoxCutter.Velocity.Y, (float)delta)
                );
            }
            _BoxCutter.MoveAndSlide();
        }

        private float CalculateVelocity(float dir, float v, float t)
        {
            if (Mathf.Abs(v) >= _MaxSpeed) return v * _Friction;
            return (dir * _MaxSpeed) + (_Acceleration * t);
        }

        protected override void OnTimerFinished()
        {
            // Stop all player input
            SetProcess(false);
            SetPhysicsProcess(false);

            // This is a failure case only, so play the sound
            PlayFailureSfx();
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 100;
            _Camera3D.HOffset = offset.X;
            _Camera3D.VOffset = offset.Y;
        }

        private void OnTapeCut()
        {
            // Screenshake
            _Screenshake.Shake(
                ScreenshakeHandler.ShakePayload.ShakeSizes.Mild,
                ScreenshakeHandler.ShakeAxis.XOnly
            );

            // Microsleep
            EmitSignal(nameof(Microsleep));

            // If all tape is cut, then end the microgame
            if (_Tapes.All(t => t.IsTapeCut)) WinMicrogame();
        }

        private async void WinMicrogame()
        {
            // Stop all player input
            SetProcess(false);
            SetPhysicsProcess(false);

            // Stop timer
            MicrogameTimer.Stop();

            // Set outcome
            Outcome = Outcomes.Win;

            // Play sfx
            PlaySuccessSfx();

            // Fade out box cutter
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_BoxCutter, "modulate:a", 0f, 0.25f);

            // Run animations
            _BoxAnimPlayer.Play("top_open");
            await ToSignal(_BoxAnimPlayer, "animation_finished");

            // Emit outcome
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }
    }
}
