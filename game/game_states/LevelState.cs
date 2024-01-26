using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Levels;
using ShopIsDone.Utils.Extensions;
using System.Threading.Tasks;
using ShopIsDone.Core.Data;

namespace ShopIsDone.Game.States
{
    public partial class LevelState : State
    {
        [Export]
        public string LevelOverride;

        // Nodes
        private Level _Level;
        private Events _Events;
        private LevelDb _LevelDb;

        public override void _Ready()
        {
            base._Ready();
            _Events = Events.GetEvents(this);
            _LevelDb = LevelDb.GetLevelDb(this);

            // Connect to level change requested
            _Events.LevelChangeRequested += OnLevelChangeRequested;
        }

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Pull the initial level from the message
            var levelId = (string)message.GetValueOrDefault(Consts.LEVEL_KEY);
            if (string.IsNullOrEmpty(levelId)) levelId = LevelOverride;
            await InitLevel(levelId);

            base.OnStart(message);
        }

        public async override void OnExit(string nextState)
        {
            await CleanUpLevel();
            base.OnExit(nextState);
        }

        private async Task InitLevel(string levelId)
        {
            // Load level scene
            // TODO: Put loading screen up until this is done
            var levelData = _LevelDb.Levels[levelId];
            var levelScene = GD.Load<PackedScene>(levelData.LevelScenePath);

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
            // Run clean up
            _Level.CleanUp();

            // Fade to black
            _Events.EmitSignal(nameof(_Events.FadeInRequested));
            await ToSignal(_Events, nameof(_Events.FadeInFinished));

            // Remove scene
            RemoveChild(_Level);
            _Level.QueueFree();
        }

        private async void OnLevelChangeRequested(string levelId)
        {
            await CleanUpLevel();
            await InitLevel(levelId);
        }
    }
}

