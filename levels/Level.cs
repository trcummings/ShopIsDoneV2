using Godot;
using System;
using ShopIsDone.Arenas;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Cameras;
using ShopIsDone.Widgets;
using ShopIsDone.Utils;

namespace ShopIsDone.Levels
{
    public partial class Level : Node
    {
        [Export]
        private StateMachine _LevelStateMachine;

        // Services
        [Export]
        private CameraService _CameraService;

        [Export]
        private InputXformer _InputXformer;

        [Export]
        private FingerCursor _FingerCursor;

        [Export]
        private TileCursor _TileCursor;

        [Export]
        private ScreenshakeService _Screenshake;

        [Export]
        private DirectionalInputHelper _DirectionalInput;

        [Export]
        private TileIndicator _TileIndicator;

        [Export]
        private MovePathWidget _MovePathWidget;

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            // Ready nodes
            _InjectionProvider = InjectionProvider.GetProvider(this);

            // Wire up arena entrances
            var entrances = GetTree().GetNodesInGroup("arena_entrance").OfType<EnterArenaArea>();
            foreach (var entrance in entrances)
            {
                entrance.EnteredArena += (arena) => OnPlayerEnteredArena(entrance, arena);
            }

            // Register all services (NB: We have to do this manually, unfortunately)
            _InjectionProvider.RegisterService(_CameraService);
            _InjectionProvider.RegisterService(_InputXformer);
            _InjectionProvider.RegisterService(_FingerCursor);
            _InjectionProvider.RegisterService(_TileCursor);
            _InjectionProvider.RegisterService(_Screenshake);
            _InjectionProvider.RegisterService(_DirectionalInput);
            _InjectionProvider.RegisterService(_TileIndicator);
            _InjectionProvider.RegisterService(_MovePathWidget);
        }

        public void Init()
        {
            // Change state to initializing state
            _LevelStateMachine.ChangeState(Consts.States.INITIALIZING_STATE);
        }

        private void OnPlayerEnteredArena(EnterArenaArea area, Arena arena)
        {
            _LevelStateMachine.ChangeState(Consts.States.ARENA_STATE, new Dictionary<string, Variant>()
            {
                { Consts.States.ARENA_KEY, arena },
                { Consts.States.ENTER_ARENA_AREA_KEY, area }
            });
        }
    }
}

