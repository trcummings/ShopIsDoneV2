using Godot;
using Godot.Collections;
using System;

namespace ShopIsDone.EntityStates
{
    public partial class ExitedEntityState : EntityState
    {
        public override void Enter(Dictionary<string, Variant> message = null)
        {
            base.Enter(message);
            _Entity.Hide();
        }

        public override void Exit()
        {
            base.Exit();
            _Entity.Show();
        }
    }
}

