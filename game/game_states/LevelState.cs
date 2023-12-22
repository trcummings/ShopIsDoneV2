using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Levels;

namespace ShopIsDone.Game.States
{
    public partial class LevelState : State
    {
        [Export]
        public PackedScene LevelScene;

        [Export]
        public Godot.Environment LevelEnvironment;

        // Nodes
        private Level _Level;
        private GlobalEvents _GlobalEvents;

        public override void _Ready()
        {
            base._Ready();
            _GlobalEvents = GlobalEvents.GetGlobalEvents(this);
        }

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Set render environment
            _GlobalEvents.EmitSignal(nameof(_GlobalEvents.ChangeEnvironmentRequested), LevelEnvironment);

            // Ready scene
            _Level = LevelScene.Instantiate<Level>();
            AddChild(_Level);

            // TODO: Connect to level events

            // Init level
            _Level.Init();

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

            // Remove scene
            RemoveChild(_Level);
            _Level.QueueFree();

            base.OnExit(nextState);
        }
    }
}

