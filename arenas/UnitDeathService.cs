using Godot;
using ShopIsDone.Cameras;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Arenas
{
    // This is for removing units on death safely from the arena without freeing
    // their memory
    public partial class UnitDeathService : Node, IService
    {
        [Inject]
        private TileManager _TileManager;

        [Inject]
        private CameraService _CameraService;

        public void Init()
        {
            InjectionProvider.Inject(this);
        }

        public void SafelyRemoveUnit(LevelEntity entity)
        {
            if (_CameraService.GetCameraTarget() == entity)
            {
                // Set camera target to tile the unit is on
                var lastTile = _TileManager.GetTileAtTilemapPos(entity.TilemapPosition);
                _CameraService.SetCameraTarget(lastTile).Execute();
            }

            // Disable body collder
            entity.GetNode<CollisionShape3D>("CollisionShape").Disabled = true;

            // Move to far off point
            entity.GlobalPosition = Vec3.FarOffPoint;
        }
    }
}

