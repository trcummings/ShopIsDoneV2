using System;

namespace ShopIsDone.Core
{
	public interface IEntityActiveHandler
	{
		bool IsActive();

		bool IsInArena();
	}
}

