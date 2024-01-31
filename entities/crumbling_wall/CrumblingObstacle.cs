using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Entities.CrumblingObstacle
{
	public partial class CrumblingObstacle : NodeComponent, IOnCleanupComponent
    {
		[Export]
		private CollisionShape3D _Collider;

        [Export]
        private AnimationPlayer _AnimPlayer;

        [Export]
		private int _MaxCrumbleTurns = 3;

		[Export]
        private int _TurnsRemaining = 3;

        [Inject]
        private TileManager _TileManager;

        public override void Init()
        {
            base.Init();
			InjectionProvider.Inject(this);
        }

		private Dictionary<int, string> _HealthToAnim = new Dictionary<int, string>()
		{
			{ 0, "crush_3" },
            { 1, "crush_2" },
            { 2, "crush_1" },
            { 3, "default" },
        };

        // On each turn, check if there's a nearby unit that can move,
        // and crumble by one turn if so. if we're at zero, remove the collision
        // layer
		public void TickCrumble()
		{
			// Ignore if we've already crumbled
			if (_TurnsRemaining == 0) return;

			if (HasAdjacentUnit())
			{
				_TurnsRemaining -= 1;
				_TurnsRemaining = Mathf.Max(0, _TurnsRemaining);

				// If we've run out, disable the collider
				if (_TurnsRemaining == 0) _Collider.Disabled = true;
				// Play next crumble animation
				_AnimPlayer.Play(_HealthToAnim[_TurnsRemaining]);
			}
		}

		public Command OnCleanup()
		{
            return new ActionCommand(TickCrumble);
		}

		private bool HasAdjacentUnit()
		{
			var currentTile = _TileManager.GetTileAtTilemapPos(Entity.TilemapPosition);
			var neighbors = currentTile.FindNeighbors();

			return neighbors.Values.Any(tile => tile.HasUnitOnTile());
		}
	}
}

