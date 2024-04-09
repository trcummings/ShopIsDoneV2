using Godot;
using System;
using ShopIsDone.Cameras;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using Utils.Extensions;
using ShopIsDone.Utils.StateMachine;
using SystemGenerics = System.Collections.Generic;
using ShopIsDone.Utils;

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
        private Array<Marker3D> _OverstockItems = new Array<Marker3D>();
        private Array<StockArea> _StockAreas = new Array<StockArea>();
        private ReturnsCart _ReturnsCart;
        private Node2D _DropAreas;
        private Node3D _StockItems;
        private StockItem _WeirdStockItem;

        private Vector2 _StockAreaSize = new Vector2(340, 120);
        private Vector2 _ReturnAreaSize = new Vector2(672, 252);

        private Array<StockArea> _UsedStockAreas = new Array<StockArea>();
        private enum ExtraCases
        {
            Null,
            Missing,
            WrongPosition,
            WeirdItem,
        }

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Camera3D = GetNode<Camera3D>("%Camera3D");
            _StateMachine = GetNode<StateMachine>("%StateMachine");
            _DropAreas = GetNode<Node2D>("%DropAreas");
            _StockItems = GetNode<Node3D>("%StockItems");
            _ReturnsCart = GetNode<ReturnsCart>("%ReturnsCart");

            _OverstockItems = GetNode<Node3D>("%OverstockItems")
                .GetChildren()
                .OfType<Marker3D>()
                .ToGodotArray();

            _StockAreas = GetNode<Node3D>("%StockAreas")
                .GetChildren()
                .OfType<StockArea>()
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
            // Tween in return cart
            var finalCartPos = _ReturnsCart.Position.X;
            _ReturnsCart.Position = _ReturnsCart.Position with { X = 5 };
            var cartTween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Bounce);
            cartTween.TweenProperty(_ReturnsCart, "position:x", finalCartPos, 1f);
            // Rattle cart on finish
            cartTween.TweenCallback(Callable.From(() => {
                _ReturnsCart.EmitSignal(nameof(_ReturnsCart.CartRattled));

                // Create cart return area
                var returnArea = CreateArea(
                    _ReturnsCart.DropzoneMarker.GlobalPosition,
                    _ReturnAreaSize
                );
                _ReturnsCart.Init(returnArea, _WeirdStockItem);
                _ReturnsCart.DroppedItemInCart += DroppedItemInCart;
            }));

            /// Set overstock items
            var shuffledItemScenes = StockItemScenes.ToList().Shuffle().ToList();
            // Create stock items at each point
            var overstockItems = new Array<StockItem>();
            for (int i = 0; i < shuffledItemScenes.Count; i++)
            {
                var point = _OverstockItems[i];
                var scene = shuffledItemScenes[i];
                var item = scene.Instantiate<StockItem>();
                point.AddChild(item);
                overstockItems.Add(item);
                item.Init(i);
            }

            // Create stock area data
            for (int i = 0; i < shuffledItemScenes.Count; i++)
            {
                var area = _StockAreas[i];
                var item = overstockItems[i];

                // Create dropzone for stock area
                var areaPos = area.GlobalPosition with { Y = area.GlobalPosition.Y - 0.1f };
                var area2D = CreateArea(areaPos, _StockAreaSize);
                // Initialize the stock area
                area.Init(_StockItems, area2D, item);
            }

            // Remove an item from a stock area
            var shuffledAreas = new SystemGenerics.Stack<StockArea>(_StockAreas.ToList().Shuffle());
            var firstArea = shuffledAreas.Pop();
            firstArea.DeleteAnItem();
            _UsedStockAreas.Add(firstArea);

            // Create a weird stock item, just in case
            _WeirdStockItem = WeirdStockItemScene.Instantiate<StockItem>();
            _WeirdStockItem.Init(-1);
            _WeirdStockItem.Position = Vec3.FarOffPoint;
            _StockItems.AddChild(_WeirdStockItem);

            //Pick one of four different cases
            var selectedCase = Enum.GetValues(typeof(ExtraCases))
                .OfType<ExtraCases>()
                .ToList()
                .PickRandom();
            switch (selectedCase)
            {
                case ExtraCases.Null:
                    {
                        // No additional changes necessary
                        break;
                    }

                case ExtraCases.Missing:
                    {
                        // Pick another stock area from the unused stock areas
                        // and delete an item
                        var nextArea = shuffledAreas.Pop();
                        nextArea.DeleteAnItem();
                        _UsedStockAreas.Add(nextArea);
                        break;
                    }

                case ExtraCases.WrongPosition:
                    {
                        // Move one item from one stock area to the empty one
                        var nextArea = shuffledAreas.Pop();
                        var item = nextArea.StockItems.PickRandom();
                        nextArea.RemoveItem(item);
                        firstArea.AddItem(item);
                        _UsedStockAreas.Add(nextArea);
                        break;
                    }

                case ExtraCases.WeirdItem:
                    {
                        // Add the weird item to the first area
                        firstArea.AddItem(_WeirdStockItem);
                        break;
                    }
            }

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
            // Set collision
            area2D.SetCollisionMaskValue(1, false);
            area2D.SetCollisionLayerValue(1, false);
            area2D.SetCollisionMaskValue(2, true);
            // Set shape
            var shape2D = new CollisionShape2D();
            _DropAreas.AddChild(area2D);
            area2D.AddChild(shape2D);
            shape2D.GlobalPosition = pos;
            // Adjust collision shape position
            shape2D.Position -= new Vector2(0, size.Y / 2);
            shape2D.Shape = new RectangleShape2D() { Size = size };

            return area2D;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Check all stock areas are filled
            if (_StockAreas.All(a => a.IsFull()))
            {
                WinMicrogame();
            }
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
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 100;
            _Camera3D.HOffset = offset.X;
            _Camera3D.VOffset = offset.Y;
        }

        private async void DroppedItemInCart(StockItem item)
        {
            if (item.Id != _WeirdStockItem.Id)
            {
                // Fail game with screenshake
                _Screenshake.Shake(
                    ScreenshakeHandler.ShakePayload.ShakeSizes.Mild,
                    ScreenshakeHandler.ShakeAxis.XOnly
                );

                // Stop all player input
                SetPlayerCanProcess(false);

                // Stop timer
                MicrogameTimer.Stop();

                // Set outcome
                Outcome = Outcomes.Loss;

                await ToSignal(GetTree().CreateTimer(.5f), "timeout");

                // Play sfx
                PlayFailureSfx();

                // Emit outcome
                EmitSignal(nameof(MicrogameFinished), (int)Outcome);
            }
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
