using Godot;
using Godot.Collections;
using ShopIsDone.EntityStates;
using System;

namespace ShopIsDone.Entities.ClownPuppet
{
    public partial class ClownPuppetHiddenState : EntityState
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

