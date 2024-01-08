using Godot;
using ShopIsDone.Core;
using System;

namespace ShopIsDone.Actions
{
    public partial class TeamHandler : NodeComponent
    {
        [Export]
        public Teams Team;
        public enum Teams
        {
            Player,
            Enemy,
            Neutral
        }

        public bool IsOnSameTeam(LevelEntity entity)
        {
            return entity?.GetComponent<TeamHandler>()?.Team == Team;
        }
    }
}

