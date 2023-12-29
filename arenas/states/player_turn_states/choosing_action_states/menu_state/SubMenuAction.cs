using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu
{
    // Menu action that contains menu actions
    public partial class SubMenuAction : MenuAction
    {
        // Title of Sub Menu
        public string Title = "";

        // List of actions that this submenu contains
        public List<MenuAction> MenuActions = new List<MenuAction>();

        // If any sub-actions are visible, then this one is too
        public override bool IsVisible()
        {
            return MenuActions.Any(action => action.IsVisible());
        }

        // If any sub-actions are available, then this one is too
        public override bool IsAvailable()
        {
            return MenuActions.Any(action => action.IsAvailable());
        }

        public override void SelectAction(MenuState actionState)
        {
            // Transition to choosing menu action state but with a different payload
        }
    }
}