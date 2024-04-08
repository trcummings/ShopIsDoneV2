using System;

namespace ShopIsDone.Microgames.DownStock
{
	public interface IHoverable
	{
		void Hover();

		void Unhover();
	}

	public interface IDropzone : IHoverable
	{
		bool IsDropzoneHovered();

		bool CanDrop(StockItem item);

		void Drop(StockItem item);
    }
}

