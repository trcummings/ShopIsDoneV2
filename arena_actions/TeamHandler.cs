using Godot;
using ShopIsDone.Core;
using System;
using static ShopIsDone.Actions.ArenaAction;

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

        public bool CanActAgainst(DispositionTypes targetDisposition, LevelEntity entity)
        {
            // If our target has no team, then we can't act against them
            if (!entity?.HasComponent<TeamHandler>() ?? false) return false;
            // If the targeted disposition is towards any, then we can
            else if (targetDisposition == DispositionTypes.Any) return true;

            // Get entity team
            var entityTeam = entity?.GetComponent<TeamHandler>()?.Team;

            // If we're on the same team and it's friendly, we can
            if (entityTeam == Team && targetDisposition == DispositionTypes.Friends) return true;

            // If we're on opposite teams and it's hostile, then we can
            var onOppositeTeams =
                (entityTeam == Teams.Player && Team == Teams.Enemy) ||
                (Team == Teams.Player && entityTeam == Teams.Enemy);
            if (onOppositeTeams && targetDisposition == DispositionTypes.Enemies) return true;

            // Otherwise, one is neutral and the other isn't, or both are neutral,
            // and we can't
            return false;
        }
    }
}

