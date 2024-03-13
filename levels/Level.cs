using Godot;
using System;
using ShopIsDone.Arenas;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Levels
{
    public partial class Level : Node
    {
        [Signal]
        public delegate void InitializedEventHandler();

        [Export]
        private StateMachine _LevelStateMachine;

        // Services
        private ServicesContainer _Services;

        public override void _Ready()
        {
            base._Ready();
            // Ready nodes
            _Services = GetNode<ServicesContainer>("%Services");

            // Register and init all services
            _Services.RegisterServices();
            _Services.InitServices();
        }

        public void CleanUp()
        {
            // Go to exiting state if we weren't already in there
            if (_LevelStateMachine.CurrentState != Consts.States.EXITING)
            {
                _LevelStateMachine.ChangeState(Consts.States.EXITING);
            }

            // Unregister and clean up all services
            _Services.UnregisterServices();
            _Services.CleanUpServices();
        }

        public const string ON_FINISHED_INIT = "OnFinishedInit";
        public const string ON_PLAYER_ENTERED_ARENA = "OnPlayerEnteredArena";

        public void Init()
        {
            // Change state to initializing state
            _LevelStateMachine.ChangeState(Consts.States.INITIALIZING, new Dictionary<string, Variant>()
            {
                { ON_FINISHED_INIT, Callable.From(() => EmitSignal(nameof(Initialized))) },
                { ON_PLAYER_ENTERED_ARENA, new Callable(this, nameof(OnPlayerEnteredArena)) }
            });
        }

        private void OnPlayerEnteredArena(EnterArenaArea area, Arena arena)
        {
            _LevelStateMachine.ChangeState(Consts.States.ARENA, new Dictionary<string, Variant>()
            {
                { Consts.States.ARENA_KEY, arena },
                { Consts.States.ENTER_ARENA_AREA_KEY, area }
            });
        }
    }
}

