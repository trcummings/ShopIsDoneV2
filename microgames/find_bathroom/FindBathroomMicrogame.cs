using Godot;
using System;
using ShopIsDone.Cameras;
using static ShopIsDone.Cameras.ScreenshakeHandler;
using ShopIsDone.Utils.Inputs;
using Godot.Collections;

namespace ShopIsDone.Microgames.FindBathroom
{
    public partial class FindBathroomMicrogame : Microgame
    {
        [Export]
        public PackedScene MoveNodeScene;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private TileMap _TileMap;
        private Node2D _EmployeeCustomerPair;
        private InputBuffer _InputBuffer;

        // Tweens
        private Tween _PairTween;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _TileMap = GetNode<TileMap>("%TileMap");
            _EmployeeCustomerPair = GetNode<Node2D>("%EmployeeCustomerPair");
            _InputBuffer = GetNode<InputBuffer>("%InputBuffer");

            // Connect screenshake
            _Screenshake.Connect(
                nameof(ScreenshakeHandler.ShakeOffsetUpdated),
                new Callable(this, nameof(ShakeUpdate))
            );

            // Do not allow player input
            SetProcess(false);
            SetPhysicsProcess(false);
        }

        public override void Init(Dictionary<string, Variant> msg)
        {
            base.Init(msg);

            // Place move nodes

            // Place employee customer pair

            // Place exit

            // Place obstacles

            // Place cart spawners
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
            // If pair is moving, ignore input
            if (_PairTween.IsRunning()) return;

            // Otherwise check buffered input for if we pressed a direction within
            // the last 8 frames
            var latestAction = _InputBuffer.GetMostRecentlyPressedAction(_InputBuffer.Actions);
            if (!string.IsNullOrEmpty(latestAction) && _InputBuffer.WasInputPressedWithin(latestAction, 8 * (float)delta))
            {
                // Get the available move node in that direction

                // If there isn't one, emit a failure signal and give some mild
                // screenshake

                // If there is one, tween the employee customer pair to that node
                // and face them in that direction

            }
        }

        protected override void OnTimerFinished()
        {
            // Play success sound if we won
            if (Outcome == Outcomes.Win) PlaySuccessSfx();
        }

        private void FailGame()
        {
            // Stop all player input
            SetProcess(false);
            SetPhysicsProcess(false);

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();

            // Finish early
            FinishEarly();
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 1000;
        }
    }
}