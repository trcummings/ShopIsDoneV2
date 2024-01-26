using System;

namespace ShopIsDone.Game
{
	public static class Consts
	{
		// Groups
		public const string WORLD_VIEWPORT = "world_viewport";

		// Game State Message keys
		public const string OVERRIDE_GAME_STATE = "OverrideGameState";
		public const string INITIAL_LEVEL = "InitialLevel";
        public const string LEVEL_KEY = "Level";

        // Game States
        public class GameStates
		{
			public const string INITIAL_LOAD = "InitialLoad";
			public const string VANITY_CARD = "VanityCard";
            public const string MAIN_MENU = "MainMenu";
            public const string LEVEL = "Level";
        }
	}
}

