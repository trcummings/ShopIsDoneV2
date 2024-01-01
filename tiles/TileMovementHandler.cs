using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Pathfinding;

namespace ShopIsDone.Tiles
{
    // This component helps generate movement on the tilemap for all moveable
    // units
    public partial class TileMovementHandler : NodeComponent
    {
        [Signal]
        public delegate void TurnedToFaceDirEventHandler(Vector3 dir);

        [Export]
        private CharacterBody3D _Body;

        [Export]
        public CommandProcessor _CommandProcessor;

        [Export]
        public UnitTileMoveValidator _MoveValidator;

        [Export]
        public SkipTileStrategyNode _SkipTileStrategy;

        // Properties
        [Export]
        // This describes how many tiles the unit can move every turn at baseline
        public int BaseMove = 3;

        [Export]
        // This has to do with the speed at which the actual unit moves in the
        // movement animation
        public double MovementSpeed = 10;

        [Export]
        public float MoveEffortMod = 0.6f; // (3 -> 5 ratio)

        [Export]
        public bool CanPassThroughAllies = false;

        [Export]
        public bool CanSeeInTheDark = false;

        public void Init(EntityManager _)
        {
            InjectionProvider.Inject(this);
        }

        public List<Tile> GetAvailableMoves(Tile fromTile, bool includeFromTile = true, int moveRange = -1)
        {
            return new MoveGenerator(_SkipTileStrategy).GetAvailableMoves(
                fromTile,
                includeFromTile,
                // If the given move range is less than zero, use our base move range
                // This is in order to keep the function flexible
                moveRange < 0 ? BaseMove : moveRange
            );
        }

        public bool IsValidMovePath(List<Tile> movePath)
        {
            return _MoveValidator.IsValidMovePath(movePath);
        }

        // NB: Assume that MovePath will always have two or more tiles in it, that
        // the first tile should always be the current position of the unit, and
        // also assume that each tile will be neighbors with the next. We can also
        // assume that the movement will never end with the unit on a tile with
        // another unit on it. A path will also never loop in on itself.
        public List<Command> GetMoveCommands(List<Tile> movePath)
        {
            // Break each move to each tile on the list into a series of commands
            // NB: We only want to look to the second last value of the list because
            // we're looking forward
            return movePath
                // Turn into list of prev-next entries
                .WithPrevious(null)
                // Skip first where prev value is null
                .SkipWhile((record) => record.Previous == null)
                // Aggregate into a collection of commands if the movement has the
                // pawn passing through another pawn or not
                .Aggregate(new List<List<(Tile, Tile)>>(), (acc, record) =>
                {
                    // Find out if we must "pass through" this tile
                    var mustPassThrough = _SkipTileStrategy.MustPassThroughTile(record.Previous);

                    // If the first tile is not a pass through tile, then create a
                    // new entry to map over
                    if (!mustPassThrough) acc.Add(new List<(Tile, Tile)>() { record });
                    // Otherwise, append the current entry to the previous one
                    else
                    {
                        // If there's no prior entry in the accumulator, add an
                        // empty one
                        if (acc.Count == 0) acc.Add(new List<(Tile, Tile)>());

                        // Grab the prior entry in the accumulator
                        // NB: This will exist because of what we just did up there
                        var last = acc.Last();

                        // Add the current pair to that list
                        last.Add(record);
                    }

                    return acc;
                })
                // Map into a list of commands
                .Select(records =>
                {
                    // If it's a single command, just pass through as normal
                    if (records.Count() == 0)
                    {
                        var record = records.First();
                        return GetMoveCommand(record.Item1, record.Item2);
                    }

                    // Otherwise, pull out the first-most tile and the last most tile
                    var firstTile = records.First().Item1;
                    var lastTile = records.Last().Item2;

                    return new PawnMoveSeriesCommand()
                    {
                        Movement = this,
                        Entity = Entity,
                        InitialTile = firstTile,
                        FinalTile = lastTile,
                        CommandProcessor = _CommandProcessor,
                        // Map the sub-movements to a series of commands
                        SeriesCommand = new SeriesCommand(
                            records.Select(record => GetMoveCommand(record.Item1, record.Item2)).ToArray()
                        )
                    };
                })
                .ToList<Command>();
        }

        protected virtual PawnMoveCommand GetMoveCommand(Tile currentTile, Tile nextTile)
        {
            // If the height is the same between the two, add a move command
            if (currentTile.TilemapPosition.Y == nextTile.TilemapPosition.Y)
            {
                // Get new facing direction
                var newFacingDir = (nextTile.TilemapPosition - currentTile.TilemapPosition).Sign();
                // Define a series command so we can perform an animation and set
                // the facing direction as commands rather than as mutations
                return new PawnMoveSeriesCommand()
                {
                    Movement = this,
                    Entity = Entity,
                    InitialTile = currentTile,
                    FinalTile = nextTile,
                    CommandProcessor = _CommandProcessor,
                    SeriesCommand = new SeriesCommand(
                        // Set the pawn facing direction to the new direction
                        new ActionCommand(() => EmitSignal(nameof(TurnedToFaceDir), newFacingDir)),
                        // Move between the two
                        new MoveBetweenTilesCommand()
                        {
                            Movement = this,
                            Entity = Entity,
                            InitialTile = currentTile,
                            FinalTile = nextTile,
                            CommandProcessor = _CommandProcessor
                        }
                    )
                };
            }
            // If there's a height difference, add a jump move between the two
            // NB: We can assume the player's jump height doesn't matter here
            // because this is a valid move path
            else
            {
                return new JumpBetweenTilesCommand()
                {
                    Movement = this,
                    Entity = Entity,
                    InitialTile = currentTile,
                    FinalTile = nextTile,
                    CommandProcessor = _CommandProcessor
                };
            }


            // TODO: other commands like having to climb at the cost of a move
            // if the pawn isn't athletic, or if they can vault over a surface
            // instead of having to jump
        }

        // Movement commands
        public partial class PawnMoveCommand : Command, IPhysicsProcessableCommand
        {
            public TileMovementHandler Movement;
            public LevelEntity Entity;
            public CommandProcessor CommandProcessor;
            public Tile InitialTile;
            public Tile FinalTile;

            public virtual void PhysicsProcess(double delta) { }
        }

        public partial class PawnMoveSeriesCommand : PawnMoveCommand
        {
            public SeriesCommand SeriesCommand;

            public override void Execute()
            {
                SeriesCommand.Finished += OnFinishSeries;
                SeriesCommand.Execute();
            }

            private void OnFinishSeries()
            {
                SeriesCommand.Finished -= OnFinishSeries;
                Finish();
            }
        }

        private partial class MoveBetweenTilesCommand : PawnMoveCommand
        {
            // Undo vars
            private Vector3 _PrevGlobalTranslation;

            public override void Execute()
            {
                // Set undo vars
                _PrevGlobalTranslation = Entity.GlobalPosition;

                // Set the command to the process node
                CommandProcessor.AddCommand(this);
            }

            public override void PhysicsProcess(double delta)
            {
                // Lerp towards the final position
                Entity.GlobalPosition = Entity.GlobalPosition.Lerp(
                    FinalTile.GlobalPosition,
                    (float)(Movement.MovementSpeed * delta)
                );

                // Check if we're "close enough" (less than 0.05)
                if (Entity.GlobalPosition.DistanceTo(FinalTile.GlobalPosition) < 0.05)
                {
                    // Set the exact position
                    Entity.GlobalPosition = FinalTile.GlobalPosition;

                    // remove the process command
                    CommandProcessor.RemoveCommand(this);

                    // Emit finished signal
                    Finish();
                }
            }
        }

        private partial class JumpBetweenTilesCommand : PawnMoveCommand
        {
            public override void Execute()
            {
                throw new NotImplementedException();
            }

            public override void PhysicsProcess(double delta)
            {
                throw new NotImplementedException();
            }
        }
    }
}
