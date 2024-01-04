using Godot;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Actors.States;
using Godot.Collections;

namespace ShopIsDone.Actors
{
    public partial class Actor : LevelEntity
    {
        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private StateMachine _ActorStateMachine;

        [Export]
        private FreeMoveActorState _FreeMoveState;

        [Export]
        private FollowLeaderActorState _FollowLeaderState;

        public void Init(IActorInput actorInput)
        {
            _FreeMoveState.Init(actorInput);
            _ActorAnimator.Init();
            _ActorStateMachine.ChangeState(Consts.States.FREE_MOVE);
        }

        public void FollowLeader(Node3D leader)
        {
            _ActorStateMachine.ChangeState(Consts.States.FOLLOW_LEADER, new Dictionary<string, Variant>()
            {
                { Consts.LEADER_KEY, leader }
            });
        }

        public void EnterArena()
        {
            _ActorStateMachine.ChangeState(Consts.States.IN_ARENA);
        }

        public void ExitArena()
        {
            _ActorStateMachine.ChangeState(Consts.States.FREE_MOVE);
        }
    }
}
