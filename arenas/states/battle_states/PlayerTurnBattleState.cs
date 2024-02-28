using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using Consts = ShopIsDone.Arenas.PlayerTurn.Consts;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas.Battles.States
{
	public partial class PlayerTurnBattleState : State
	{
        [Export]
        private StateMachine _PlayerTurnStateMachine;

        [Export]
        public PlayerUnitService _PlayerUnitService;

        public override void _Ready()
        {
            base._Ready();

            // Initialize battle state machine
            _PlayerTurnStateMachine.ChangeState(Consts.States.IDLE);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Dependency injection
            InjectionProvider.Inject(this);

            // Save temp arena data at start of turn
            //SaveTempArenaData();

            // If we have no active units, end the turn
            if (!_PlayerUnitService.HasUnitsThatCanStillAct())
            {
                _PlayerTurnStateMachine.ChangeState(Consts.States.ENDING_TURN);
                return;
            }

            // Go to the choosing unit state
            _PlayerTurnStateMachine.ChangeState(Consts.States.CHOOSING_UNIT);
        }

        public void Interrupt()
        {
            // Turn state machine to idle
            _PlayerTurnStateMachine.ChangeState(Consts.States.IDLE);
        }

        public void Resume()
        {
            // Resume previous state
            var (prevState, prevMessage) = _PlayerTurnStateMachine.GetLastStateProps();
            _PlayerTurnStateMachine.ChangeState(prevState, prevMessage);
        }

        public override void OnExit(string nextState)
        {
            _PlayerTurnStateMachine.ChangeState(Consts.States.IDLE);
            base.OnExit(nextState);
        }
    }
}
