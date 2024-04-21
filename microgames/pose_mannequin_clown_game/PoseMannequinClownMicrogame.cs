using Godot;
using System;
using ShopIsDone.Cameras;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Microgames.PoseMannequin
{
    public partial class PoseMannequinClownMicrogame : Microgame
    {
        [Signal]
        public delegate void MicrosleepEventHandler();

        // Nodes
        private Camera3D _Camera;
        private ScreenshakeHandler _Screenshake;

        private StateMachine _StateMachine;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera3D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _StateMachine = GetNode<StateMachine>("%StateMachine");

            // Connect screenshake
            _Screenshake.ShakeOffsetUpdated += ShakeUpdate;
        }

        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);

            // Hide the timer bar
            HideTimer(0.1f);
            // Show the background
            ShowBackground(0.5f);

            // Start state machine in hovering state
            _StateMachine.ChangeState(Consts.States.APPROACHING_MANNEQUIN, new Dictionary<string, Variant>()
            {
                { Consts.IS_FIRST_TIME_KEY, true }
            });
        }

        public override void Start()
        {
            // Override so nothing happens on start
        }

        public void InterruptTimer()
        {
            MicrogameTimer.Stop();
            HideTimer(0.1f);
        }

        public void StartTimer()
        {
            base.Start();
            ShowTimer(0.1f);
        }

        protected override void OnTimerFinished()
        {
            // Hide the timer bar
            HideTimer(0.1f);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
        }

        private void ShakeUpdate(Vector2 offset)
        {
            _Camera.HOffset = offset.X;
            _Camera.VOffset = offset.Y;
        }

        private async void WinMicrogame()
        {
            // Stop timer
            MicrogameTimer.Stop();

            // Set outcome
            Outcome = Outcomes.Win;

            await ToSignal(GetTree().CreateTimer(.5f), "timeout");

            // Play sfx
            PlaySuccessSfx();

            // Emit outcome
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }
    }
}
