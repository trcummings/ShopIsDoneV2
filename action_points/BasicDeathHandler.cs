using System;
using Godot;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDeathHandler : DeathHandler
	{
        [Export]
        protected EntityStateHandler _StateHandler;

        public override Command Die()
        {
            return _StateHandler.RunChangeState("dead");
        }
    }
}

