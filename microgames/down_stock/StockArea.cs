using Godot;
using System;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class StockArea : Node3D
    {
        public bool HasWeirdItem;

        public bool IsInOverstock;

        public bool IsEmpty;

        public bool IsMissingOne;

        public StockItem Item;

        public void Init(Area2D area2D)
        {

        }
    }
}

