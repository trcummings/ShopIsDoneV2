using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Arenas;
using System.Linq;

namespace ShopIsDone.Levels.States
{
    public partial class InitializingState : State
    {
        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Export]
        private PauseInputHandler _PauseInputHandler;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Pull callbacks out of dictionary
            var onFinishedInit = (Callable)message[Level.ON_FINISHED_INIT];
            var onPlayerEnteredArena = (Callable)message[Level.ON_PLAYER_ENTERED_ARENA];

            // Initialize player character manager
            _PlayerCharacterManager.Init();

            // Wire up arena entrances
            var entrances = GetTree().GetNodesInGroup("arena_entrance").OfType<EnterArenaArea>();
            foreach (var entrance in entrances)
            {
                entrance.EnteredArena += (arena) => onPlayerEnteredArena.Call(entrance, arena);
            }

            // Disable pausing
            _PauseInputHandler.IsActive = false;

            // Finish the start hook
            base.OnStart(message);

            // Finish initialization (to raise the curtain)
            onFinishedInit.Call();

            // Go to the free move state for now
            ChangeState(Consts.States.FREE_MOVE);
        }
    }
}

