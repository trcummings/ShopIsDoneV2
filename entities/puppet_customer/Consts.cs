using System;

namespace ShopIsDone.Entities.PuppetCustomers
{
	public static class Consts
	{
		public const string ENTITY_TARGET = "Target";
		public const string TILE_TARGET = "TileTarget";
		public const string PRIORITY_GOAL_TILES = "PriorityGoalTiles";
        public const string BOTHERED_EMPLOYEE = "BotheredEmployee";
        public const string FOLLOW_PATH_TILES = "FollowPathTiles";

        public static class States
        {
            public const string IDLE = "idle";
            public const string MOVE = "move";
            public const string DEAD = "dead";
        }

        public static class Actions
        {
            public const string BOTHER_EMPLOYEE = "bother_employee";
        }
    }
}

