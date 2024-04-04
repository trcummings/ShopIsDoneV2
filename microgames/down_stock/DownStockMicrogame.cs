using Godot;
using System;
using ShopIsDone.Cameras;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using Utils.Extensions;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class DownStockMicrogame : Microgame
    {
        [Signal]
        public delegate void MicrosleepEventHandler();

        [Export]
        public Array<PackedScene> StockItemScenes;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private Camera3D _Camera3D;

        private StateMachine _StateMachine;
        private CharacterBody2D _GrabHand;
        private Array<Marker3D> _StockItemPoints = new Array<Marker3D>();
        private Array<Node3D> _StockAreas = new Array<Node3D>();

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Camera3D = GetNode<Camera3D>("%Camera3D");
            _StateMachine = GetNode<StateMachine>("%StateMachine");

            _StockItemPoints = GetNode<Node3D>("%OverstockItems")
                .GetChildren()
                .OfType<Marker3D>()
                .ToGodotArray();

            _StockAreas = GetNode<Node3D>("%StockAreas")
                .GetChildren()
                .OfType<Node3D>()
                .ToGodotArray();

            // Get grab hand and make transparent
            _GrabHand = GetNode<CharacterBody2D>("%GrabHand");
            _GrabHand.Modulate = Colors.Transparent;

            // Connect screenshake
            _Screenshake.ShakeOffsetUpdated += ShakeUpdate;

            // Do not allow player input
            SetPlayerCanProcess(false);
        }

        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);

            // Fade in grabber hand
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_GrabHand, "modulate:a", 1f, 1f);

            /// Set overstock items
            var shuffledItemScenes = StockItemScenes.ToList().Shuffle();
            // Pick four used items from the set
            var usedItemScenes = shuffledItemScenes.Take(4).ToList();
            // Single out the remaining one
            var remainingItem = shuffledItemScenes.Skip(4).Take(1).First();
            // Create stock items at each point
            for (int i = 0; i < usedItemScenes.Count(); i++)
            {
                var point = _StockItemPoints[i];
                var scene = usedItemScenes[i];
                var item = scene.Instantiate<Node3D>();
                point.AddChild(item);
                // Add some mild random rotation to the stock items
                item.RotateY((float)GD.RandRange(-Mathf.Pi / 10, Mathf.Pi / 10)); 
            }

            /// Set stock areas 
            // Pick 2 stock areas, and fill them fully
            // Partially fill the other 2 stock areas



            // Pick game condition

            // 3 game conditions:
            // Return then replace
            // Pick 
            // Fill up empty items

            // Return an out of place item + stock
            // Replace 2 items for an empty area (process of elimination)

            // Start state machine in hovering state
            _StateMachine.ChangeState(Consts.States.HOVERING);
        }

        public override void Start()
        {
            base.Start();

            // Allow player input
            SetPlayerCanProcess(true);
        }

        private void SetPlayerCanProcess(bool value)
        {
            _StateMachine.SetProcess(value);
            _StateMachine.SetPhysicsProcess(value);
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
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 100;
            _Camera3D.HOffset = offset.X;
            _Camera3D.VOffset = offset.Y;
        }

        private void WinMicrogame()
        {
            // Stop all player input
            SetPlayerCanProcess(false);

            // Stop timer
            MicrogameTimer.Stop();

            // Set outcome
            Outcome = Outcomes.Win;

            // Play sfx
            PlaySuccessSfx();

            // Emit outcome
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }
    }
}
