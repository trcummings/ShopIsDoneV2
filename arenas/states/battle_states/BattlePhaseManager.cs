using Godot;
using ShopIsDone.Utils.StateMachine;
using System;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using System.Linq;

namespace ShopIsDone.Arenas.Battles
{
    // This class helps control changing to different phases
    public partial class BattlePhaseManager : Node
    {
        [Signal]
        public delegate void PhaseChangedEventHandler(string newPhase);

        [Export]
        public State CurrentPhase;

        [Export]
        public Array<State> PhaseOrder;

        [Export]
        private StateMachine _BattleStateMachine;

        public override void _Ready()
        {
            // Do not process until we've been initialized
            SetProcess(false);
            _BattleStateMachine.ChangeState("Idle");
        }

        public void Init()
        {
            // Start processing after we've been initialized
            SetProcess(true);
        }

        public void Stop()
        {
            SetProcess(false);
            _BattleStateMachine.ChangeState("Idle");
        }

        public override void _Process(double _)
        {
            // Ignore if the state machine is in sync with the current phase
            if (CurrentPhase.Name == _BattleStateMachine.CurrentState) return;
            // If it isn't, update the state machine
            _BattleStateMachine.ChangeState(CurrentPhase.Name);
            EmitSignal(nameof(PhaseChanged), CurrentPhase.Name);
        }

        public void AdvanceToNextPhase()
        {
            // Find the current phase in the phase order and get the next phase
            // circularly
            var idx = PhaseOrder.ToList().FindIndex(s => s == CurrentPhase);
            var nextPhase = PhaseOrder.SelectCircular(idx, 1);

            // Set Current Phase as the next phase (it will update on next process)
            CurrentPhase = nextPhase;
        }
    }
}
