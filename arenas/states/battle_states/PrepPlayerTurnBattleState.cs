using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Tasks;
using ShopIsDone.Actions;

namespace ShopIsDone.Arenas.Battles.States
{
	public partial class PrepPlayerTurnBattleState : State
	{
        [Export]
        private BattlePhaseManager _PhaseManager;

        [Export]
        private UnitTurnService _PlayerUnitTurnService;

        [Export]
        private TaskService _TaskService;

        [Export]
        private ActionService _ActionService;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            var command = new SeriesCommand(
                // Update silhouettes

                // Resolve status effects
                _PlayerUnitTurnService.ResolveEffects(),

                // Refill AP
                new ActionCommand(_PlayerUnitTurnService.RefillApToMax),

                // Resolve in progress tasks
                _TaskService.ResolveInProgressTasks(),

                // Tick status effect durations
                _PlayerUnitTurnService.TickEffectDurations(),

                // Update arena
                _ActionService.PostActionUpdate()
            );
            // Advance on finished
            command.Connect(
                nameof(command.Finished),
                Callable.From(_PhaseManager.AdvanceToNextPhase),
                (uint)ConnectFlags.OneShot
            );
            command.Execute();
        }
    }
}
