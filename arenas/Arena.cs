using Godot;
using Godot.Collections;
using System;
using ShopIsDone.Utils.Commands;
using Camera;
using ShopIsDone.Tiles;
using System.Threading.Tasks;
using ShopIsDone.Core;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas
{
    public partial class ArenaAction : GodotObject
    {

    }

    public partial class ActionRule : GodotObject
    {

    }

    public partial class DialogPayload : GodotObject
    {

    }

    public partial class CharacterData : GodotObject
    {

    }

    public partial class Arena : Node3D
    {
        [Export]
        private TileManager _TileManager;

        [Export]
        public StateMachine _ArenaStateMachine;

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public void Init(LevelEntity playerUnit)
        {
            _TileManager.Init();

            // Move units into arena
            var placementTiles = GetTree()
                .GetNodesInGroup("placement_tile")
                .OfType<Tile>()
                .Where(IsAncestorOf);
            var placement = placementTiles.First();
            playerUnit.GlobalPosition = placement.GlobalPosition;

            // Register services
            _InjectionProvider.RegisterService(_TileManager);

            // Change state to initializing
            _ArenaStateMachine.ChangeState("Initializing");
        }

        public void CleanUp()
        {
            // Unregister services
            _InjectionProvider.UnregisterService(_TileManager);
        }

        public void ExecuteAction(Command action)
        {

        }

        public Command Start()
        {
            return new Command();
        }

        public void Screenshake(ScreenshakeHandler.ShakePayload payload)
        {

        }

        public void StopScreenshake()
        {

        }

        // Entity placement
        public Command PlaceEntity()
        {
            return new Command();
        }

        // Sandbox methods
        public Task CreateDialog(DialogPayload payload)
        {
            return Task.Run(() => { });
        }

        // Characters
        public CharacterData GetCharacter(string id)
        {
            return new CharacterData();
        }

        // Phase Management
        public Command AdvanceToNextPhase()
        {
            return new Command();
        }

        public Command ChangeArenaState(string stateName)
        {
            return new Command();
        }

        public Command AdvanceToVictoryPhase()
        {
            return new Command();
        }

        public Command AdvanceToOutOfTimePhase()
        {
            return new Command();
        }

        public Command AdvanceToDefeatPhase()
        {
            return new Command();
        }

        // Arena Updates
        public Command UpdateEntities()
        {
            return new Command();
        }

        public Command ResetPawnActions()
        {
            return new Command();
        }

        public Command RemovePawnsNotInArena()
        {
            return new Command();
        }

        public Command UpdateAI()
        {
            return new Command();
        }

        public Command UpdateArena()
        {
            return new Command();
        }

        public Command ExecuteAction(LevelEntity pawn, ArenaAction action, Dictionary<string, Variant> message = null)
        {
            return new Command();
        }

        private Command RunScriptQueue()
        {
            return new Command();
        }

        // Counter Actions
        public Command RunCounterActions(LevelEntity pawn, ArenaAction action, Dictionary<string, Variant> message = null)
        {
            return new Command();
        }

        #region Rules
        public Command ShowBrokenRuleOnList(ActionRule rule)
        {
            return new Command();
        }

        public Array<ActionRule> GetBrokenActionRules()
        {
            return new Array<ActionRule>();
        }

        public Command ProcessActionRules(LevelEntity pawn, ArenaAction action, Dictionary<string, Variant> message = null)
        {
            return new Command();
        }

        // Map over each action rule and reset all action rules
        public Command ResetActionRules()
        {
            return new Command();
        }
        #endregion

        // Audio
        public Command PlaySound(string soundId)
        {
            return new Command();
        }

        // Tasks
        public Command ResolveInProgressTasks()
        {
            return new Command();
        }

        // Win / Fail functionality
        public bool IsPlayerVictorious()
        {
            return false;
        }

        public bool WasPlayerDefeated()
        {
            return false;
        }


        public Command ExitPawnFromArena(LevelEntity pawn)
        {
            return new Command();
        }

        public Command PunishPinkSlip(LevelEntity pawn)
        {
            return new Command();
        }

        public Command SwapEmployeeForMannequin(LevelEntity pawn)
        {
            return new Command();
        }

        public Command ShowDemerit(LevelEntity pawn)
        {
            return new Command();
        }

        // Camera
        public Command SetCameraTarget(Node3D target)
        {
            return new Command();
        }

        public Node3D GetCameraTarget()
        {
            return new Node3D();
        }

        public Command WaitForCameraToReachTarget(Node3D target)
        {
            return new Command();
        }

        public Command PanToTemporaryCameraTarget(Command next)
        {
            return new Command();
        }

        public Command TemporaryCameraZoom(Command next)
        {
            return new Command();
        }

        public Command ZoomCameraTo(float zoomAmount)
        {
            return new Command();
        }

        public Command RotateCameraTo(Vector3 facingDir)
        {
            return new Command();
        }

        public void WarpCameraTo(Vector3 position)
        {

        }

        public void SetCameraMoveDuration(float newDuration)
        {

        }

        public void ResetCameraMoveDuration()
        {

        }

        // Scripting Utilities
        // This function is an idle frame Task so you can just await normally
        public async Task WaitFrame()
        {
            await ToSignal(GetTree(), "idle_frame");
        }

        // This function is a timer Task so you can just await normally
        public async Task WaitFor(float duration)
        {
            await ToSignal(GetTree().CreateTimer(duration), "timeout");
        }

        // UI
        public Command ShowTransientUI(Control node, float duration = 2.25F)
        {
            return new Command();
        }


        // Widgets
        // Tile Indicator
        public void ShowTileIndicators(System.Collections.Generic.IEnumerable<Tile> tiles, Color color)
        {

        }

        public void HideTileIndicators()
        {

        }
    }
}

