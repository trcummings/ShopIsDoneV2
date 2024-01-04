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
		}

		public static class Customers
		{
			public const string INTIMIDATE = "intimidate";
			public const string BOTHER = "bother";
		}
	}
}

