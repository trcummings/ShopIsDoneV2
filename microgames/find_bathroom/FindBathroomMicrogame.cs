using Godot;
using System;
using ShopIsDone.Cameras;
using static ShopIsDone.Cameras.ScreenshakeHandler;
using ShopIsDone.Utils.Inputs;
using Godot.Collections;
using System.Linq;
using SystemGenerics = System.Collections.Generic;
using Utils.Extensions;

namespace ShopIsDone.Microgames.FindBathroom
{
    public partial class FindBathroomMicrogame : Microgame
    {
        [Signal]
        public delegate void InvalidMoveEventHandler();

        [Export]
        public PackedScene MoveNodeScene;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private TileMap _TileMap;
        private EmployeeCustomerPair _EmployeeCustomerPair;
        private InputBuffer _InputBuffer;
        private Node2D _MoveNodes;

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

            // Place exit
            //var exitSpot = GetCellPositions(2).ToList().PickRandom();

            // Place obstacles

            // Place cart spawners
        }

        private SystemGenerics.IEnumerable<Vector2> GetCellPositions(int layer)
        {
            return _TileMap.GetUsedCells(layer).Select(cell =>
            {
                var localPos = _TileMap.MapToLocal(cell);
                return _TileMap.ToGlobal(localPos);
            });

        }

        public override void Start()
        {
            //base.Start();

            // Allow player input
            SetProcess(true);
            SetPhysicsProcess(true);
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

                    // Microsleep
                    EmitSignal(nameof(InvalidMove));
                }
                // If there is one, tween the employee customer pair to that node
                // and face them in that direction
                else
                {
                    var newNode = neighbors[dir];
                    _CurrentMoveNode = newNode;
                    _EmployeeCustomerPair.MoveTo(dir, newNode.GlobalPosition);
                }
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