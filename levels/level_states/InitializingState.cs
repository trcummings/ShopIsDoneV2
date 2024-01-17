using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Arenas;
using System.Linq;
using ShopIsDone.Interactables;

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

            // Wire up break room entranecs
            var breakRooms = GetTree().GetNodesInGroup("break_room").OfType<Area3D>();
            foreach (var breakRoom in breakRooms)
            {
                breakRoom.BodyEntered += (Node3D _) =>
                {
                    ChangeState(Consts.States.BREAK_ROOM);
                };
            }

            // Wire up interactables
            var interactables = GetTree().GetNodesInGroup("interactables").OfType<Interactable>();
            foreach (var interactable in interactables) interactable.Init();

            // Disable pausing
            _PauseInputHandler.IsActive = false;

            // Finish the start hook
            base.OnStart(message);

            // Finish initialization (to raise the curtain)
            onFinishedInit.Call();

            // TODO: Pick next initial state based on some hitherto unknown
            // factor
            ChangeState(Consts.States.FREE_MOVE);
        }
    }
}

