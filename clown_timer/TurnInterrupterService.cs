using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.Battles.States;
using ShopIsDone.Core;
using System;
using System.Threading.Tasks;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas.ClownTimer
{
    // Arena service to interrupt a turn midway through. This should be used
    // only in conjunction with the clown timer to determine when it's safe to
    // interrupt a turn
	public partial class TurnInterrupterService : Node, IService
    {
        [Signal]
        public delegate void FinishedInterruptingEventHandler();

        [Export]
        private PlayerTurnBattleState _PlayerTurnBattleState;

        [Export]
        private ActionService _ActionService;

        public async Task RunInterrupter(ArenaAction action, Dictionary<string, Variant> msg = null)
        {
            // Interrupt the player's turn
            _PlayerTurnBattleState.Interrupt();

            // Run the action
            var command = _ActionService.ExecuteAction(action, new Dictionary<string, Variant>());
            command.CallDeferred(nameof(command.Execute));
            await ToSignal(command, nameof(command.Finished));

            // Resume the player's turn
            _PlayerTurnBattleState.Resume();

            // Finish
            EmitSignal(nameof(FinishedInterrupting));
        }
    }
}

