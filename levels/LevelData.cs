using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Levels
{
    // This is a node that tracks flags for the level, saves and loads them
    public partial class LevelData : Node, IService
    {
        [Export]
        public Dictionary<string, bool> Flags = new Dictionary<string, bool>();
    }
}

