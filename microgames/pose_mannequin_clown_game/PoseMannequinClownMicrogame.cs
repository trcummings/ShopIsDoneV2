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

        [Export]
        public Array<PackedScene> StockItemScenes;

        [Export]
        public PackedScene WeirdStockItemScene;

        // Nodes
        private ScreenshakeHandler _Screenshake;
        private Camera3D _Camera3D;

        private StateMachine _StateMachine;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Camera3D = GetNode<Camera3D>("%Camera3D");
            _StateMachine = GetNode<StateMachine>("%StateMachine");

            // Connect screenshake
            _Screenshake.ShakeOffsetUpdated += ShakeUpdate;

            // Do not allow player input
            SetPlayerCanProcess(false);
        }

        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);

            // Start state machine in hovering state
            _StateMachine.ChangeState(Consts.States.CHOOSING_POSE);
        }

        public override void Start()
        {
            base.Start();

            // Allow player input
            SetPlayerCanProcess(true);
        }


        public override void _Process(double delta)
        {
            base._Process(delta);
        }

        private void SetPlayerCanProcess(bool value)
        {
            _StateMachine.SetProcess(value);
            _StateMachine.SetPhysicsProcess(value);
            SetPhysicsProcess(value);
            SetProcess(value);
        }

        protected override void OnTimerFinished()
        {
            // Stop all player input
            SetPlayerCanProcess(false);

            // This is a failure case only, so play the sound
            PlayFailureSfx();

            // Emit
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private void ShakeUpdate(Vector2 offset)
        {
            _Camera3D.HOffset = offset.X;
            _Camera3D.VOffset = offset.Y;
        }

        private async void WinMicrogame()
        {
            // Stop all player input
            SetPlayerCanProcess(false);

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
