using Godot;
using ShopIsDone.Utils.Extensions;
using System;
using System.Linq;
using Godot.Collections;

namespace ShopIsDone.Core
{
    [Tool]
    public partial class EntityIdHelper : Node
    {
        public override string[] _GetConfigurationWarnings()
        {
            return GetTree()
                .GetNodesInGroup("entities")
                .OfType<LevelEntity>()
                .Aggregate(new Dictionary<string, int>(), (acc, val) => {
                    if (acc.ContainsKey(val.Id)) acc[val.Id] += 1;
                    else acc.Add(val.Id, 1);
                    return acc;
                })
                .Where(kv => kv.Value > 1)
                .Select(kv => $"Entities with duplicate with Id \"{kv.Key}\" found")
                .ToArray();
        }
    }
}

