using Godot;
using System;

namespace ShopIsDone.Arenas.PlayerTurn
{
	public static class Consts
	{
        public const string LAST_SELECTED_TILE_KEY = "LastSelectedTile";
        public const string SELECTED_UNIT_KEY = "SelectedUnit";

        public static class States
		{
			public const string IDLE = "Idle";
			public const string CHOOSING_UNIT = "ChoosingUnit";
			public const string CHOOSING_ACTION = "ChoosingAction";
			public const string ENDING_TURN = "EndingTurn";
			public const string RUNNING_ACTION = "RunningAction";
		}
    }
}
