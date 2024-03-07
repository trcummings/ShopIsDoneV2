using System;

namespace ShopIsDone.Entities.ClownPuppet
{
	public static class Consts
	{
		public static class States
		{
            public const string IDLE = "idle";
            public const string HIDDEN = "hidden";
        }

		public static class Actions
		{
            public const string WARP_IN = "warp_in";
            public const string WARP_OUT = "warp_out";
            public const string PUNISH = "punish";
            public const string FINISH_PUNISH = "finish_punish";
        }
	}
}

