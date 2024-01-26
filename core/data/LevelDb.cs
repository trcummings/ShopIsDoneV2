using Godot;
using System;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Core.Data
{
    // This is a simple autoload for dealing with what levels are in the game
    [Tool]
    [GlobalClass]
    public partial class LevelDb : Node
    {
        // Static function to help get the singleton
        public static LevelDb GetLevelDb(Node node)
        {
            return node.GetNode<LevelDb>("/root/LevelDb");
        }

        // ID for the first level you go to on new game
        [Export]
        public string NewGameLevel;

        // ID for the break room level
        [Export]
        public string BreakRoomLevel = "break_room_level";

        [Export]
        public Dictionary<string, LevelDbItem> Levels = new Dictionary<string, LevelDbItem>();

        public bool HasLevelWithId(string id)
        {
            return Levels.ContainsKey(id);
        }

        public Array<LevelDbItem> GetLevels()
        {
            return Levels
                .Values
                .Where(level => level.Id != BreakRoomLevel)
                .ToGodotArray();
        }
    }
}
