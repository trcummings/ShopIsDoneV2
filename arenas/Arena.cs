using Godot;
using ShopIsDone.Tiles;
using ShopIsDone.Core;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.DependencyInjection;
using ArenaConsts = ShopIsDone.Arenas.States.Consts;
using ShopIsDone.Levels;

namespace ShopIsDone.Arenas
{
    // The purpose of the Arena is to register Arena-specific Services with the
    // Injection Container, and to clean up when the Arena is finished
    public partial class Arena : Node3D
    {
        [Signal]
        public delegate void ArenaFinishedEventHandler();

        [Signal]
        public delegate void ArenaFailedEventHandler();

        [Signal]
        public delegate void ArenaEnteredEventHandler();

        [Export]
        public StateMachine _ArenaStateMachine;

        [Export]
        private TileManager _TileManager;

        // NB: Technically a service, but it lives under Level
        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        // Services
        private ServicesContainer _Services;

        public override void _Ready()
        {
            base._Ready();
            _Services = GetNode<ServicesContainer>("%Services");
        }

        public void Init()
        {
            _Services.RegisterServices();
            _Services.InitServices();

            // Move units into arena
            _PlayerCharacterManager.EnterArena();

            // Change state to initializing
            _ArenaStateMachine.ChangeState(ArenaConsts.INITIALIZING);

            EmitSignal(nameof(ArenaEntered));
        }

        public void CleanUp()
        {
            // Unregister services
            _Services.UnregisterServices();
            _Services.CleanUpServices();

            // Idle player units
            _PlayerCharacterManager.Idle();

            // Clean up tile manager tiles
            _TileManager.CleanUp();
        }

        public void FinishArena()
        {
            EmitSignal(nameof(ArenaFinished));
        }

        public void FailArena()
        {
            EmitSignal(nameof(ArenaFailed));
        }
    }
}

