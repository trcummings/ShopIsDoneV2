using System;

namespace ShopIsDone.Actors.States
{
	public static class Consts
	{
		public const string LEADER_KEY = "Leader";

		public static class States
		{
			public const string IN_ARENA = "InArena";
			public const string FREE_MOVE = "FreeMove";
			public const string FOLLOW_LEADER = "FollowLeader";
		}
	}
}

