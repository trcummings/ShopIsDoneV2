using Godot;
using System;
using ShopIsDone.Tiles;

namespace ShopIsDone.Selectables
{
    public partial class SelectableTile : ComponentTileArea
    {
        // The associated selectable
        public Selectable Selectable;
    }
}