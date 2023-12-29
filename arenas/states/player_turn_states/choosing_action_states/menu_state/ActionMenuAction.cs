using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Actions;
using ShopIsDone.Tiles;
using ShopIsDone.Core;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu
{
    public partial class ActionMenuAction : MenuAction
    {
        public LevelEntity Pawn;

        // Getter for the specific action related to the pawn
        public ArenaAction Action
        {
            get { return GetPawnAction(); }
        }

        public override bool IsAvailable()
        {
            return Pawn.GetComponent<ActionHandler>().IsActionAvailable(Action);
        }

        // TODO: Coalesce this code between this and CustomerPawnActionPlanner.cs
        public virtual ArenaAction GetPawnAction()
        {
            // Print a "not implemented!" error but don't throw
            GD.PrintErr("GetPawnAction not implemented for ", GetType());

            // Return a base non-functional pawn action
            return new ArenaAction() { Entity = Pawn };
        }

        // This is a function for all ActionMenuActions to use under the hood
        // when implementing GetPawnAction (non-generic)
        protected ArenaAction GetPawnAction<T>() where T : ArenaAction
        {
            // Find the pawn action by type
            return Pawn.GetComponent<ActionHandler>()
                .Actions
                .OfType<T>()
            .FirstOrDefault()
                // But nullish coalesce if we can't find it
                ?? new ArenaAction() { Entity = Pawn };
        }

        /* This function is for targeting. Does this curernt tile contain a valid 
         * target? */
        public virtual bool ContainsValidTarget(Tile tile)
        {
            return false;
        }
    }
}
