using System;
using Godot;

namespace ShopIsDone.Arenas
{
    // Simple class to track turn state for arenas
    public partial class ArenaTurnCounter : Node
    {
        [Export]
        public bool HasTurnCounter = true;

        [Export]
        public bool IsFirstTurn = true;

        [Export]
        public int TurnsRemaining = 20;

        [Export]
        public int MaxTurnsRemaining = 20;

        // Public API
        public void TickDown()
        {
            // If it's not the first turn, tick down
            if (!IsFirstTurn) TurnsRemaining -= 1;
            // Otherwise, make it no longer the first turn
            else IsFirstTurn = false;
        }

        public bool IsOutOfTime()
        {
            // If remaining turns is 0 (or somehow less)
            return TurnsRemaining <= 0;
        }
    }
}
