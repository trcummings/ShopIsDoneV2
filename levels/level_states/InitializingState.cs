using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Arenas;
using System.Linq;
using ShopIsDone.Interactables;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Cameras;

namespace ShopIsDone.Levels.States
{
    public partial class InitializingState : State
    {
        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Export]
        private PauseInputHandler _PauseInputHandler;

        [Export]
        private CameraService _CameraService;

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

            // TODO: Load in data

            // Finish the start hook
            base.OnStart(message);

            // Finish initialization (to raise the curtain)
            onFinishedInit.Call();

            // Activate camera service
            _CameraService.Init();
            _CameraService.SetCameraTarget(_PlayerCharacterManager.Leader).Execute();

            // Pull auto-start arena out of message
            var autoArena = (Arena)message.GetValueOrDefault(Level.AUTO_ENTER_ARENA);

            // If we're not given an auto arena to start, go into free move state
            if (autoArena?.Equals(default(Variant)) ?? true)
            {
                ChangeState(Consts.States.FREE_MOVE);
            }
            // Otherwise, start the arena immediately
            else
            {
                ChangeState(Consts.States.ARENA, new Dictionary<string, Variant>()
                {
                    { Consts.States.ARENA_KEY, autoArena },
                });
            }
        }
    }
}

