using System;

namespace ShopIsDone.Actors.States
{
	public static class Consts
	{
		public const string LEADER_KEY = "Leader";
		public const string INPUT_KEY = "ActorInput";

		public static class States
		{
			public const string IDLE = "Idle";
			public const string IN_ARENA = "InArena";
			public const string FREE_MOVE = "FreeMove";
			public const string FOLLOW_LEADER = "FollowLeader";
		}
	}
}

