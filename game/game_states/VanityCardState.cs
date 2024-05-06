using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Game.States
{
    public partial class VanityCardState : State
    {
        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // TODO: Run vanity card(s)

            // Change state to main menu
            ChangeState(Consts.GameStates.MAIN_MENU);
        }
    }
}