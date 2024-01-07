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
using ShopIsDone.Microgames;
using ShopIsDone.Pausing;

namespace ShopIsDone.Levels
{
    public partial class Level : Node
    {
        [Signal]
        public delegate void InitializedEventHandler();

        [Export]
        private StateMachine _LevelStateMachine;

        // Services
        [Export]
        private CameraService _CameraService;

        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Export]
        private PlayerCameraService _PlayerCameraService;

        [Export]
        private InputXformer _InputXformer;

        [Export]
        private PauseInputHandler _PauseInputHandler;

        [Export]
        private ScreenshakeService _Screenshake;

        [Export]
        private DirectionalInputHelper _DirectionalInput;

        [Export]
        private FingerCursor _FingerCursor;

        [Export]
        private TileCursor _TileCursor;

        [Export]
        private TileIndicator _TileIndicator;

        [Export]
        private MovePathWidget _MovePathWidget;

        [Export]
        private FacingWidget _FacingWidget;

        [Export]
        private MicrogameController _MicrogameController;

        [Export]
        private CutsceneService _CutsceneService;

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            // Ready nodes
            _InjectionProvider = InjectionProvider.GetProvider(this);

            // Register all services (NB: We have to do this manually, unfortunately)
            _InjectionProvider.RegisterService(_CameraService);
            _InjectionProvider.RegisterService(_PlayerCharacterManager);
            _InjectionProvider.RegisterService(_InputXformer);
            _InjectionProvider.RegisterService(_PauseInputHandler);
            _InjectionProvider.RegisterService(_Screenshake);
            _InjectionProvider.RegisterService(_DirectionalInput);
            _InjectionProvider.RegisterService(_FingerCursor);
            _InjectionProvider.RegisterService(_TileCursor);
            _InjectionProvider.RegisterService(_TileIndicator);
            _InjectionProvider.RegisterService(_MovePathWidget);
            _InjectionProvider.RegisterService(_FacingWidget);
            _InjectionProvider.RegisterService(_PlayerCameraService);
            _InjectionProvider.RegisterService(_MicrogameController);
            _InjectionProvider.RegisterService(_CutsceneService);
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

