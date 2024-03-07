using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Models;
using ShopIsDone.Utils.StateMachine;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Actors.States
{
    public partial class InArenaActorState : State
    {
        [Export]
        private NodePath _ModelPath;
        private IModel _Model;

        public override void _Ready()
        {
            _Model = GetNode<IModel>(_ModelPath);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            _Model.PerformAnimation(StateConsts.IDLE);
            base.OnStart(message);
        }
    }
}

