using Godot;
using System;
using ShopIsDone.Cameras;
using static ShopIsDone.Cameras.ScreenshakeHandler;
using ShopIsDone.Utils.Inputs;
using Godot.Collections;
using System.Linq;
using SystemGenerics = System.Collections.Generic;
using Utils.Extensions;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Microgames.FindBathroom
{
    public partial class FindBathroomMicrogame : Microgame
    {
        [Signal]
        public delegate void MicrosleepEventHandler();

        [Signal]
        public delegate void HitEventHandler();

        [Signal]
        public delegate void InvalidMoveEventHandler();

        [Signal]
        public delegate void MovedEventHandler();

        [Export]
        public PackedScene MoveNodeScene;

        [Export]
        public PackedScene BlockadeScene;

        [Export]
        public PackedScene ExitScene;

        [Export]
        public PackedScene BackFreezerShelvesScene;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private TileMap _TileMap;
        private EmployeeCustomerPair _EmployeeCustomerPair;
        private InputBuffer _InputBuffer;
        private Node2D _MoveNodes;
        private Array<CartSpawner> _Spawners;
        private Node2D _Arena;
        private Node2D _Background;

        // State
        private MoveNode _CurrentMoveNode;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _TileMap = GetNode<TileMap>("%TileMap");
            _EmployeeCustomerPair = GetNode<EmployeeCustomerPair>("%EmployeeCustomerPair");
            _InputBuffer = GetNode<InputBuffer>("%InputBuffer");
            _MoveNodes = GetNode<Node2D>("%MoveNodes");
            _Background = GetNode<Node2D>("%Background");
            _Arena = GetNode<Node2D>("%Arena");
            _Spawners = _Arena
                .GetChildren()
                .OfType<CartSpawner>()
                .ToGodotArray();

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
            foreach (var pos in GetCellPositions(0))
            {
                var moveNode = MoveNodeScene.Instantiate<MoveNode>();
                _MoveNodes.AddChild(moveNode);
                moveNode.GlobalPosition = pos;
            }

            // Place employee customer pair
            var startSpot = GetCellPositions(1).ToList().PickRandom();
            _EmployeeCustomerPair.GlobalPosition = startSpot;
            _CurrentMoveNode = _MoveNodes
                .GetChildren()
                .OfType<MoveNode>()
                .ToList()
                .Find(node => node.GlobalPosition == startSpot);
            // Connect to events
            _EmployeeCustomerPair.CustomerWasStruck += StrikeCustomer;
            _EmployeeCustomerPair.EmployeeWasStruck += StrikeEmployee;
            _EmployeeCustomerPair.ReachedBathroom += WinGame;

            // Place 2 blockades on each row
            var bottomRow = GetCellPositions(3).ToList().Shuffle().Take(3);
            var topRow = GetCellPositions(4).ToList().Shuffle().Take(3);
            var blockadePoints = topRow.Concat(bottomRow);
            foreach (var point in blockadePoints)
            {
                var blockade = BlockadeScene.Instantiate<StaticBody2D>();
                _Arena.AddChild(blockade);
                blockade.GlobalPosition = point;
            }

            // Gather all exit sposts
            var exitSpots = GetCellPositions(2).ToList();
            var exitSpot = exitSpots.PickRandom();
            // Place non-exit backgrounds
            foreach (var (prev, spot, next) in exitSpots.WithPreviousAndNext(Vector2.Inf, Vector2.Inf))
            {
                var shelves = BackFreezerShelvesScene.Instantiate<Node2D>();
                _Background.AddChild(shelves);
                shelves.GlobalPosition = spot;
                var sprite = shelves.GetNode<Sprite2D>("Sprite");
                if (spot == exitSpot) sprite.Frame = 3;
                else if (prev == exitSpot) sprite.Frame = 0;
                else if (next == exitSpot) sprite.Frame = 2;
                else sprite.Frame = 1;
            }

            // Place exit
            var exit = ExitScene.Instantiate<Area2D>();
            _Background.AddChild(exit);
            exit.GlobalPosition = exitSpot;
        }

        public override void Start()
        {
            base.Start();

            // Allow player input
            SetProcess(true);
            SetPhysicsProcess(true);

            // Start carts
            foreach (var spawner in _Spawners)
            {
                spawner.Start(1f + (float)GD.RandRange(.5f, 1f));
            }
        }

        public override void _Process(double delta)
        {
            // If pair is moving, ignore input
            if (_EmployeeCustomerPair.IsRunning()) return;

            // Otherwise check buffered input for if we pressed a direction within
            // the last 8 frames
            var latestAction = _InputBuffer.GetMostRecentlyJustPressedAction(_InputBuffer.Actions);
            if (!string.IsNullOrEmpty(latestAction) && _InputBuffer.WasInputJustPressedWithin(latestAction, 250))
            {
                // Pull the neighbors of the current move node
                var neighbors = _CurrentMoveNode.GetNeighbors();

                // Get the available move node in that direction
                // NB: It will never be 0
                Vector2 dir = Vector2.Zero;
                if (latestAction == "move_up") dir = Vector2.Up;
                else if (latestAction == "move_left") dir = Vector2.Left;
                else if (latestAction == "move_right") dir = Vector2.Right;
                else if (latestAction == "move_down") dir = Vector2.Down;

                // If there isn't one, emit a failure signal and give some mild
                // screenshake
                if (!neighbors.ContainsKey(dir))
                {
                    // Screenshake
                    _Screenshake.Shake(ShakePayload.ShakeSizes.Tiny, ShakeAxis.XOnly);

                    // Emit
                    EmitSignal(nameof(InvalidMove));
                }
                // If there is one, tween the employee customer pair to that node
                // and face them in that direction
                else
                {
                    var newNode = neighbors[dir];
                    _CurrentMoveNode = newNode;
                    _EmployeeCustomerPair.MoveTo(dir, newNode.GlobalPosition);

                    // Emit
                    EmitSignal(nameof(Moved));
                }
            }
        }

        private void StrikeEmployee()
        {
            EmitSignal(nameof(Microsleep));
            EmitSignal(nameof(Hit));
            // Screenshake
            _Screenshake.Shake(ShakePayload.ShakeSizes.Mild, ShakeAxis.XOnly);
            FailGame();
        }

        private void StrikeCustomer()
        {
            EmitSignal(nameof(Microsleep));
            EmitSignal(nameof(Hit));
            // Screenshake
            _Screenshake.Shake(ShakePayload.ShakeSizes.Mild, ShakeAxis.XOnly);
            FailGame();
        }

        protected override void OnTimerFinished()
        {
            _EmployeeCustomerPair.IdlePair();
            _EmployeeCustomerPair.Urinate();

            FailGame();
        }

        private void WinGame()
        {
            // Stop timer
            MicrogameTimer.Stop();

            // Disconnect immediately
            _EmployeeCustomerPair.CustomerWasStruck -= StrikeCustomer;
            _EmployeeCustomerPair.EmployeeWasStruck -= StrikeEmployee;
            _EmployeeCustomerPair.ReachedBathroom -= WinGame;

            // Stop all player input
            SetProcess(false);
            SetPhysicsProcess(false);

            // Set outcome to win
            Outcome = Outcomes.Win;

            // Play failure sound
            PlaySuccessSfx();

            // Emit outcome
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private async void FailGame()
        {
            // Stop timer
            MicrogameTimer.Stop();

            // Disconnect immediately
            _EmployeeCustomerPair.CustomerWasStruck -= StrikeCustomer;
            _EmployeeCustomerPair.EmployeeWasStruck -= StrikeEmployee;
            _EmployeeCustomerPair.ReachedBathroom -= WinGame;

            // Stop everything
            _EmployeeCustomerPair.Stop();
            foreach (var spawner in _Spawners) spawner.Stop();

            // Stop all player input
            SetProcess(false);
            SetPhysicsProcess(false);

            await ToSignal(GetTree().CreateTimer(2f), "timeout");

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();

            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

            // Emit outcome
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 1000;
        }

        private SystemGenerics.IEnumerable<Vector2> GetCellPositions(int layer)
        {
            return _TileMap.GetUsedCells(layer).Select(cell =>
            {
                var localPos = _TileMap.MapToLocal(cell);
                return _TileMap.ToGlobal(localPos);
            });
        }
    }
}