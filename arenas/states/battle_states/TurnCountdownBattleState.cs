using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Arenas.Turns;
using ShopIsDone.Utils.UI;
using ArenaStateConsts = ShopIsDone.Arenas.States.Consts;
using FinishedConsts = ShopIsDone.Arenas.States.Finished.Consts;

namespace ShopIsDone.Arenas.Battles.States
{
    public partial class TurnCountdownBattleState : State
    {
        [Export]
        private ControlTweener _ControlTweener;

        [Export]
        private TurnsRemainingPanel _TurnsPanel;

        [Export]
        private TurnCounter _TurnCounter;

        [Export]
        private BattlePhaseManager _PhaseManager;

        [Export]
        private StateMachine _ArenaStateMachine;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Grab if it's the first turn or not before we do anything
            var isFirstTurn = _TurnCounter.IsFirstTurn;

            new IfElseCommand(
                // Tracking turns check
                () => !_TurnCounter.HasTurnCounter,

                // Advance early
                new ActionCommand(_PhaseManager.AdvanceToNextPhase),

                // Otherwise, progress normally
                new SeriesCommand(
                    // Tick turn count down
                    new ActionCommand(_TurnCounter.TickDown),

                    // If it's the first turn, just go directly to player phase
                    new IfElseCommand(
                        // First turn check
                        () => isFirstTurn,

                        // Go to next phase
                        new ActionCommand(_PhaseManager.AdvanceToNextPhase),

                        // Otherwise, check if we're out of time (deferred check)
                        new DeferredCommand(AdvanceOrRunOutOfTime)
                    )
                )
            ).Execute();
        }

        private Command AdvanceOrRunOutOfTime()
        {
            return new IfElseCommand(
                // Out of time check
                _TurnCounter.IsOutOfTime,

                // Go to out of time phase
                new ActionCommand(() => _ArenaStateMachine.ChangeState(ArenaStateConsts.FINISHED, new Dictionary<string, Variant>()
                {
                    { FinishedConsts.FINISHED_STATE_KEY, FinishedConsts.States.OUT_OF_TIME }
                })),

                // Otherwise, show remaining turn UI, then advance
                // normally
                new SeriesCommand(
                    new ActionCommand(() =>
                    {
                        // Update turns panel
                        _TurnsPanel.SetTurnsRemaining(
                            _TurnCounter.TurnsRemaining,
                            _TurnCounter.MaxTurnsRemaining
                        );
                    }),
                    new AsyncCommand(() => _ControlTweener.TweenInAsync(0.5f)),
                    new ActionCommand(_TurnsPanel.CountdownTurns),
                    new WaitForCommand(this, 2.5f),
                    new AsyncCommand(() => _ControlTweener.TweenOutAsync(0.5f)),
                    new ActionCommand(_TurnsPanel.ResetTurnPanel),
                    new ActionCommand(_PhaseManager.AdvanceToNextPhase)
                )
            );
        }
    }
}