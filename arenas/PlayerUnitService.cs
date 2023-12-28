using System;
using Godot;
using System.Collections.Generic;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas
{
	public partial class PlayerUnitService : Node, IService
    {
		public List<LevelEntity> PlayerUnits
		{ get { return _PlayerUnits; } }
		private List<LevelEntity> _PlayerUnits = new List<LevelEntity>();

		public void Init(List<LevelEntity> playerUnits)
		{
			_PlayerUnits = playerUnits;
		}
	}
}

