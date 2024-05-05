using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.TitleScreen;
using ShopIsDone.Core.Data;

namespace ShopIsDone.Game.States
{
    public partial class MainMenuState : State
    {
        [Export]
        public PackedScene TitleScreenScene;

        // Nodes
        private TitleScreenManager _TitleScreen;
        private Events _Events;

        public override void _Ready()
        {
            base._Ready();
            _Events = Events.GetEvents(this);
        }

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Ready main menu scene
            _TitleScreen = TitleScreenScene.Instantiate<TitleScreenManager>();
            AddChild(_TitleScreen);


            // Connect to title screen events
            _TitleScreen.NewGameRequested += OnNewGameRequested;
            _TitleScreen.ContinueRequested += OnContinueGameRequested;
            _TitleScreen.QuitGameRequested += () => _Events.EmitSignal(nameof(_Events.QuitGameRequested));
            var leveldb = LevelDb.GetLevelDb(this);
            _TitleScreen.BreakRoomRequested += () =>
            {
                ChangeState(Consts.GameStates.LEVEL, new Dictionary<string, Variant>()
                {
                    { Consts.LEVEL_KEY, leveldb.BreakRoomLevel }
                });
            };

            // Init title screen
            _TitleScreen.Init();

            // Fade out overlay
            _Events.EmitSignal(nameof(_Events.FadeOutRequested));
            await ToSignal(_Events, nameof(_Events.FadeOutFinished));

            base.OnStart(message);
        }

        public async override void OnExit(string nextState)
        {
            // Fade to black
            _Events.EmitSignal(nameof(_Events.FadeInRequested));
            await ToSignal(_Events, nameof(_Events.FadeInFinished));

            // Remove main menu scene
            RemoveChild(_TitleScreen);
            _TitleScreen.QueueFree();

            base.OnExit(nextState);
        }

        private void OnNewGameRequested()
        {

        }

        private void OnContinueGameRequested()
        {

        }
    }
}

