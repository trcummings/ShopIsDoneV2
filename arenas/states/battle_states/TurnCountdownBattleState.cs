using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Arenas.Battles.States
{
    public partial class TurnCountdownBattleState : State
    {
        //[OnReadyGet]
        //private TurnsRemainingUI _TurnsRemainingUI;

        // Turn management
        [Export]
        private ArenaTurnCounter _TurnCounter;

        [Export]
        private BattlePhaseManager _PhaseManager;

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
                new ActionCommand(_PhaseManager.AdvanceToNextPhase),

                // Otherwise, show remaining turn UI, then advance
                // normally
                new SeriesCommand(
                    //new ActionCommand(() =>
                    //{
                    //    // Set "Turns Remaining UI" with turns left
                    //    _TurnsRemainingUI.SetTurnsRemaining(
                    //        TurnManager.GetTurnsRemaining(),
                    //        TurnManager.GetMaxTurnsRemaining()
                    //    );
                    //}),
                    //new AsyncActionCommand(() => _TurnsRemainingUI.TweenIn(0.5f)),
                    //new ForcedSyncCommand(Arena.PlaySound("turn_tick_down")),
                    //new ActionCommand(_TurnsRemainingUI.CountdownTurns),
                    //new WaitForCommand(this, 3f),
                    //new AsyncActionCommand(() => _TurnsRemainingUI.TweenOut(0.5f)),
                    //new ActionCommand(_TurnsRemainingUI.ResetTurnPanel),
                    //Arena.AdvanceToNextPhase()
                )
            );
        }
    }
}