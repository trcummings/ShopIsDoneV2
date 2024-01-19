using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Levels;
using ShopIsDone.Utils.Extensions;
using System.Threading.Tasks;

namespace ShopIsDone.Game.States
{
    public partial class LevelState : State
    {
        [Export]
        public PackedScene LevelOverride;

        // Nodes
        private Level _Level;
        private Events _Events;

        public override void _Ready()
        {
            base._Ready();
            _Events = Events.GetEvents(this);

            // Connect to level change requested
            _Events.LevelChangeRequested += OnLevelChangeRequested;
        }

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Pull the initial level from the message
            var levelScene = (PackedScene)message.GetValueOrDefault(Consts.LEVEL_KEY);
            await InitLevel(LevelOverride ?? levelScene);

            base.OnStart(message);
        }

        public async override void OnExit(string nextState)
        {
            await CleanUpLevel();
            base.OnExit(nextState);
        }

        private async Task InitLevel(PackedScene levelScene)
        {
            // Ready scene
            _Level = levelScene.Instantiate<Level>();
            AddChild(_Level);

            // Init level
            _Level.CallDeferred(nameof(_Level.Init));
            await ToSignal(_Level, nameof(_Level.Initialized));

            // Fade out overlay
            _Events.EmitSignal(nameof(_Events.FadeOutRequested));
            await ToSignal(_Events, nameof(_Events.FadeOutFinished));
        }

        private async Task CleanUpLevel()
        {
            // Fade to black
            _Events.EmitSignal(nameof(_Events.FadeInRequested));
            await ToSignal(_Events, nameof(_Events.FadeInFinished));

            // Remove scene
            _Level.CleanUp();
            RemoveChild(_Level);
            _Level.QueueFree();
        }

        private async void OnLevelChangeRequested(PackedScene levelScene)
        {
            await CleanUpLevel();
            await InitLevel(levelScene);
        }
    }
}

