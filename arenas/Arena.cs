using Godot;
using System;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Arenas
{
    public partial class Arena : Node3D
    {
        [Export]
        private GridMap _Tiles;

        public void ExecuteAction(Command action)
        {

        }
    }
}

