using System;
using Godot;
using System.Collections.Generic;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Actions;
using System.Linq;

namespace ShopIsDone.Arenas
{
	public partial class PlayerUnitService : Node, IService
    {
		[Signal]
		public delegate void InitializedEventHandler();

		private List<LevelEntity> _PlayerUnits = new List<LevelEntity>();

		public void Init(List<LevelEntity> playerUnits)
		{
			_PlayerUnits = playerUnits;
			foreach (var unit in _PlayerUnits) unit.Init();
			EmitSignal(nameof(Initialized));
		}

        public List<LevelEntity> GetUnits()
        {
            return _PlayerUnits
                .Where(u => u.IsInArena())
                .ToList();
        }

		public bool IsPlayerUnit(LevelEntity entity)
		{
			return GetUnits().Contains(entity);
        }

		public LevelEntity GetUnitById(string id)
		{
			return _PlayerUnits.Find(e => e.Id == id);
		}

        public List<ArenaAction> GetUnitRemainingAvailableActions(LevelEntity unit)
		{
            var actionHandler = unit.GetComponent<ActionHandler>();
			// If we can no longer act, return an empty list
			if (!actionHandler.HasAvailableActions()) return new List<ArenaAction>();
			// The unit must at least have one action visible in the
			// menu, even if others are technically available
			return actionHandler
				.GetAvailableActions()
				.Where(a => a.IsVisibleInMenu())
				.ToList();
        }

		public bool UnitHasAvailableActions(LevelEntity unit)
		{
			return GetUnitRemainingAvailableActions(unit).Count > 0;
        }

		public List<ArenaAction> GetVisibleActions(LevelEntity unit)
		{
            return unit
				.GetComponent<ActionHandler>()
				.Actions
				.Where(a => a.IsVisibleInMenu())
				.ToList();
        }

		public List<LevelEntity> GetActiveUnits()
		{
			return GetUnits()
                .Where(u => u.GetComponent<ActionHandler>().HasAvailableActions())
				.ToList();
        }

		public bool HasRemainingActiveUnits()
		{
			return GetActiveUnits().Count > 0;
        }

		public List<LevelEntity> GetUnitsThatCanStillAct()
		{
			return GetActiveUnits().Where(UnitHasAvailableActions).ToList();
        }

		public bool HasUnitsThatCanStillAct()
		{
			return GetUnitsThatCanStillAct().Count > 0;
        }
	}
}

