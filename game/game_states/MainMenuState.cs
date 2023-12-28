using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.TitleScreen;

namespace ShopIsDone.Game.States
{
    public partial class MainMenuState : State
    {
        [Export]
        public PackedScene TitleScreenScene;

        // Nodes
        private TitleScreenManager _TitleScreen;
        private GlobalEvents _GlobalEvents;

        public override void _Ready()
        {
            base._Ready();
            _GlobalEvents = GlobalEvents.GetGlobalEvents(this);
        }

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Ready main menu scene
            _TitleScreen = TitleScreenScene.Instantiate<TitleScreenManager>();
            AddChild(_TitleScreen);

            // Connect to title screen events
            _TitleScreen.LevelEditorRequested += OnLevelEditorRequested;
            _TitleScreen.ContinueRequested += OnContinueGameRequested;
            _TitleScreen.QuitGameRequested += () => _GlobalEvents.EmitSignal(nameof(_GlobalEvents.QuitGameRequested));

            // Init title screen
            _TitleScreen.Init();

            // Fade out overlay
            _GlobalEvents.EmitSignal(nameof(_GlobalEvents.FadeOutRequested));
            await ToSignal(_GlobalEvents, nameof(_GlobalEvents.FadeOutFinished));

            base.OnStart(message);
        }

        public async override void OnExit(string nextState)
        {
            // Fade to black
            _GlobalEvents.EmitSignal(nameof(_GlobalEvents.FadeInRequested));
            await ToSignal(_GlobalEvents, nameof(_GlobalEvents.FadeInFinished));

            // Remove main menu scene
            RemoveChild(_TitleScreen);
            _TitleScreen.QueueFree();

            base.OnExit(nextState);
        }

        private void OnLevelEditorRequested()
        {

        }

        private void OnContinueGameRequested()
        {

        }
    }
}

