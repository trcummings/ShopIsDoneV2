using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Tiles;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Arenas;
using Generics = System.Collections.Generic;
using System.Linq;
using ShopIsDone.Cameras;
using ShopIsDone.EntityStates;
using ShopIsDone.Models;
using CustomerConsts = ShopIsDone.Entities.PuppetCustomers.Consts;
using ShopIsDone.Entities.PuppetCustomers;
using ShopIsDone.ActionPoints;
using ApConsts = ShopIsDone.ActionPoints.Consts;

namespace ShopIsDone.Entities.EntitySpawner
{
    // This "spawner" really just grabs existing and hidden entities from the
    // map and moves them into an appropriate position after a certain number of
    // turns
    public partial class EntitySpawnerComponent : NodeComponent
    {
        [Signal]
        public delegate void SpawnBlockedEventHandler();

        [Export]
        public Array<LevelEntity> Entities;

        [Export]
        public int MaxTurnsTillSpawn = 4;

        [Export]
        public int CurrentTurnsTillSpawn = 4;

        [Export]
        private SpawnPool _SpawnPool;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private ActionService _ActionService;

        [Inject]
        private CameraService _CameraService;

        // State
        private Generics.Queue<LevelEntity> _RemainingEntities;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);

            // Copy over entities to state
            _RemainingEntities = new Generics.Queue<LevelEntity>(Entities);
        }

        public Command ProgressSpawner()
        {
            var currentTile = _TileManager.GetTileAtTilemapPos(Entity.TilemapPosition);

            return new SeriesCommand(
                // Tick down next time until spawn (minimum of 0)
                new ActionCommand(() => CurrentTurnsTillSpawn = Mathf.Max(0, CurrentTurnsTillSpawn - 1)),
                // If it's time to spawn, attempt to spawn a unit on that tile
                new ConditionalCommand(
                    () => CurrentTurnsTillSpawn == 0 && _RemainingEntities.Any(),
                    new IfElseCommand(
                        // Blocked tile check
                        () => currentTile.HasObstacleOnTile || currentTile.HasUnitOnTile(),
                        // If the spawn is blocked, emit an environmental hazard
                        new SeriesCommand(
                            // Show the spawn pool & play an SFX
                            new AsyncCommand(async () =>
                            {
                                _SpawnPool.Appear();
                                _SpawnPool.Show();
                                await ToSignal(_SpawnPool, nameof(_SpawnPool.Appeared));
                                EmitSignal(nameof(SpawnBlocked));
                            }),
                            // If it's a player unit, cause an environmental hazard
                            new ConditionalCommand(
                                () => _PlayerUnitService.IsPlayerUnit(currentTile.UnitOnTile),
                                new DeferredCommand(() =>
                                {
                                    var unitOnTile = currentTile.UnitOnTile;
                                    var apHandler = unitOnTile.GetComponent<ActionPointHandler>();

                                    return apHandler.TakeAPDamage(new Dictionary<string, Variant>()
                                    {
                                        { ApConsts.DAMAGE_AMOUNT, 2 }
                                    });
                                })
                            ),
                            // Hide the spawn pool
                            new AsyncCommand(async () =>
                            {
                                _SpawnPool.Disappear();
                                await ToSignal(_SpawnPool, nameof(_SpawnPool.Disappeared));
                                _SpawnPool.Hide();
                            })
                        ),
                        // Otherwise spawn in the entity there
                        new DeferredCommand(SpawnEntity)
                    )
                )
            );
        }

        private Command SpawnEntity()
        {
            // Pop the entity off the queue
            var entity = _RemainingEntities.Dequeue();
            var stateHandler = entity.GetComponent<EntityStateHandler>();
            var model = entity.GetComponent<ModelComponent>();

            return new SeriesCommand(
                // Enable it
                new ActionCommand(() => entity.SetEnabled(true)),
                // Wait an idle frame
                new WaitIdleFrameCommand(this),
                // Init entity
                new AwaitSignalCommand(entity, nameof(entity.Initialized), nameof(entity.Init)),
                // Position it
                new ActionCommand(() =>
                {
                    // Position the entity
                    entity.GlobalPosition = Entity.GlobalPosition;
                    entity.FacingDirection = Entity.FacingDirection;
                }),
                // Wait an idle frame
                new WaitIdleFrameCommand(this),
                // Pan camera over and emerge
                _CameraService.PanToTemporaryCameraTarget(entity,
                    new SeriesCommand(
                        model.RunPerformAction(CustomerConsts.Actions.EMERGE),
                        stateHandler.RunChangeState(CustomerConsts.States.IDLE)
                    )
                ),
                // Update arena
                _ActionService.PostActionUpdate(),
                // Reset turns till spawn
                new ActionCommand(() => CurrentTurnsTillSpawn = MaxTurnsTillSpawn)
            );
        }
    }
}