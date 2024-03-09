using System;

namespace ShopIsDone.Arenas.UnitPlacement
{
    public static class Consts
	{
        public const string SELECTED_UNIT_KEY = "SelectedUnit";
        public const string INITIAL_TILE_KEY = "InitialTile";
        public const string UNITS_KEY = "Units";
        public const string TILES_KEY = "Tiles";

        public static class States
        {
		    public const string IDLE = "Idle";
            public const string SELECTING_UNIT = "SelectingUnit";
            public const string MOVING_UNIT = "MovingUnit";
        }
	}
}

