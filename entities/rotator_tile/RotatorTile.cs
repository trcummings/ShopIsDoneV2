using Godot;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Entities.RotatorTile
{
    [Tool]
    public partial class RotatorTile : NodeComponent, IOnCleanupComponent
    {
        [Inject]
        private TileManager _TileManager;

        [Export]
        private Node3D _Pivot;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
            // Set facing direction
            UpdateWidget(Entity.FacingDirection);
        }

        public Command OnCleanup()
        {
            return new ConditionalCommand(
                HasUnitOnTile,
                new ActionCommand(() =>
                {
                    var unit = GetUnitOnTile();
                    // Update unit's facing direction to be the same as ours
                    unit.FacingDirection = Entity.FacingDirection;
                })
            );
        }

        private LevelEntity GetUnitOnTile()
        {
            var currentTile = _TileManager.GetTileAtTilemapPos(Entity.TilemapPosition);
            return currentTile.UnitOnTile;
        }

        private bool HasUnitOnTile()
        {
            var currentTile = _TileManager.GetTileAtTilemapPos(Entity.TilemapPosition);
            return currentTile.HasUnitOnTile();
        }

        private void UpdateWidget(Vector3 facingDir)
        {
            _Pivot.LookAt(_Pivot.GlobalPosition + facingDir);
        }
    }
}

