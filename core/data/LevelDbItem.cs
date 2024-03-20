using Godot;
using System;

namespace ShopIsDone.Core.Data
{
    [GlobalClass]
    public partial class LevelDbItem : Resource
    {
        [Export]
        public string Id;

        [Export]
        public string Label;

        [Export]
        public bool DebugModeOnly;

        [Export(PropertyHint.File, "*.tscn")]
        public string LevelScenePath;
    }
}

