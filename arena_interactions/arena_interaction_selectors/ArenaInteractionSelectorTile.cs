using Godot;
using System;
using ShopIsDone.Tiles;

namespace ShopIsDone.ArenaInteractions.Selectors
{
    public partial class ArenaInteractionSelectorTile : ComponentTileArea
    {
        // The associated selectable
        public ArenaInteractionSelector Selectable;
        public InteractionComponent Interaction;
    }
}