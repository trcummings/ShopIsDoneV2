using Godot;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Actors.States;

namespace ShopIsDone.Actors
{
    public partial class Haskell : LevelEntity
    {
        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private StateMachine _ActorStateMachine;

        [Export]
        private FreeMoveActorState _FreeMoveState;

        public void Init(IActorInput actorInput)
        {
            _FreeMoveState.Init(actorInput);
            _ActorAnimator.Init();
            _ActorStateMachine.ChangeState("FreeMove");
        }

        public void EnterArena()
        {
            _ActorStateMachine.ChangeState("InArena");
        }

        public void ExitArena()
        {
            _ActorStateMachine.ChangeState("FreeMove");
        }
    }
}
