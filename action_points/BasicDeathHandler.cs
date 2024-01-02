using System;
using Godot;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDeathHandler : DeathHandler
	{
        [Export]
        private EntityStateHandler _StateHandler;

        public override Command Die()
        {
            // TODO: Inform the arena about a death
            return _StateHandler.RunChangeState("dead");
        }
    }
}

