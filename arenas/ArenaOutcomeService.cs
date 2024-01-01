using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Arenas
{
    public partial class ArenaOutcomeService : Node, IService
    {
        public void AdvanceToVictoryPhase()
        {

        }

        public void AdvanceToDefeatPhase()
        {

        }

        public bool IsPlayerVictorious()
        {
            return false;
        }

        public bool WasPlayerDefeated()
        {
            return false;
        }
    }
}

