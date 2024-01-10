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
        public Dictionary<string, bool> _Flags = new Dictionary<string, bool>();

        public bool GetFlag(string flagName, bool defaultValue = default)
        {
            if (!_Flags.ContainsKey(flagName))
            {
                GD.PrintErr($"In {Name}.GetFlag: No flag with name {flagName} found in LevelData");
                return defaultValue;
            }

            return _Flags[flagName];
        }

        public void SetFlag(string flagName, bool value)
        {
            if (!_Flags.ContainsKey(flagName))
            {
                GD.PrintErr($"In {Name}.SetFlag: No flag with name {flagName} found in LevelData");
            }
            _Flags[flagName] = value;
        }
    }
}

