using Godot;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu
{
    // This class is for use by the player action UI
    public partial class MenuAction : Resource
    {
        public string Name;

        // Does the action show up in the list in the first place
        public virtual bool IsVisible()
        {
            return false;
        }

        public virtual bool IsAvailable()
        {
            return false;
        }

        // This function is for the action to decide which player turn action state
        // to transition to (if any)
        public virtual void SelectAction(MenuState actionState)
        {
            // Print a "not implemented!" error but don't throw
            GD.PrintErr("SelectAction not implemented for ", this.Name);
        }
    }
}
