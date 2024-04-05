using Godot;
using System;
using ShopIsDone.Cameras;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using Utils.Extensions;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Levels;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class DownStockMicrogame : Microgame
    {
        [Signal]
        public delegate void MicrosleepEventHandler();

        [Export]
        public Array<PackedScene> StockItemScenes;

        [Export]
        public PackedScene WeirdStockItemScene;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private Camera3D _Camera3D;

        private StateMachine _StateMachine;
        private CharacterBody2D _GrabHand;
        private Array<Marker3D> _StockItemPoints = new Array<Marker3D>();
        private Array<StockArea> _StockAreas = new Array<StockArea>();
        private Marker3D _ShoppingCartMarker;
        private Node2D _DropAreas;

        private Vector2 _StockAreaSize = new Vector2(540, 140);
        private Vector2 _ReturnAreaSize = new Vector2(472, 272);

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Camera3D = GetNode<Camera3D>("%Camera3D");
            _StateMachine = GetNode<StateMachine>("%StateMachine");
            _DropAreas = GetNode<Node2D>("%DropAreas");

            _StockItemPoints = GetNode<Node3D>("%OverstockItems")
                .GetChildren()
                .OfType<Marker3D>()
                .ToGodotArray();

            _StockAreas = GetNode<Node3D>("%StockAreas")
                .GetChildren()
                .OfType<StockArea>()
                .ToGodotArray();

            _ShoppingCartMarker = GetNode<Marker3D>("%ShoppingCartMarker");

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
            var shuffledItemScenes = StockItemScenes.ToList().Shuffle().ToList();
            // Pick four used items from the set
            var usedItemScenes = shuffledItemScenes.Take(4).ToList();
            // Single out the remaining one
            var remainingItemScene = shuffledItemScenes.Skip(4).Take(1).First();
            StockItem remainingItem = null;
            // Create stock items at each point
            var overstockItems = new Array<StockItem>();
            for (int i = 0; i < usedItemScenes.Count; i++)
            {
                var point = _StockItemPoints[i];
                var scene = usedItemScenes[i];
                var item = scene.Instantiate<StockItem>();
                if (scene == remainingItemScene) remainingItem = item;
                point.AddChild(item);
                overstockItems.Add(item);
                // Add some mild random rotation to the stock items
                var randRotation = (float)GD.RandRange(- Mathf.Pi / 10, Mathf.Pi / 10);
                item.RotateY(randRotation);
            }

            // Create stock area data
            for (int i = 0; i < usedItemScenes.Count; i++)
            {
                var area = _StockAreas[i];
                var item = overstockItems[i];

                // Create dropzone for stock area
                var area2D = CreateArea(area.GlobalPosition, _StockAreaSize);
                // Initialize the stock area
                area.Init(area2D);
                // Fill with data
                area.IsInOverstock = item != remainingItem;
                area.Item = item;
            }

            // Pick one of four different cases
            // 1. Two items are missing
            // 2. One item is in the wrong position (the area it's at is missing an
            //    item)
            // 3. A weird item is present in an empty shelf and another item is
            //    missing
            // 4. A weird item is present in a filled shelf, and that newly empty
            //    spot must be replaced
            

            // Create return area
            var returnArea = CreateArea(_ShoppingCartMarker.GlobalPosition, _ReturnAreaSize);

            // Start state machine in hovering state
            _StateMachine.ChangeState(Consts.States.HOVERING);
        }

        public override void Start()
        {
            base.Start();

            // Allow player input
            SetPlayerCanProcess(true);
        }

        private Area2D CreateArea(Vector3 worldPos, Vector2 size)
        {
            var pos = _Camera3D.UnprojectPosition(worldPos);
            var area2D = new Area2D();
            var shape2D = new CollisionShape2D();
            _DropAreas.AddChild(area2D);
            area2D.AddChild(shape2D);
            shape2D.GlobalPosition = pos;
            // Adjust collision shape position
            shape2D.Position -= new Vector2(0, size.Y / 2);
            shape2D.Shape = new RectangleShape2D() { Size = size };

            return area2D;
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
