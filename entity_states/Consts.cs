using System;

namespace ShopIsDone.EntityStates
{
	public static class Consts
	{
		public const string IDLE = "idle";
        public const string ALERT = "alert";
        public const string MOVE = "move";
		public const string HURT = "hurt";
		public const string DEAD = "dead";

		public static class Employees
		{
			public const string DO_TASK = "do_task";
			public const string HELP_CUSTOMER = "help_customer";
		}

		public static class Customers
		{
			public const string INTIMIDATE = "intimidate";
			public const string BOTHER = "bother";
		}

		public static class ClownPuppet
		{
			public const string HIDDEN = "hidden";
			public const string WARP_IN = "warp_in";
            public const string WARP_OUT = "warp_out";
            public const string PUNISH = "punish";
            public const string FINISH_PUNISH = "finish_punish";
        }
	}
}

