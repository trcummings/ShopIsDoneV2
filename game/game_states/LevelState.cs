using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Levels;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Game.States
{
    public partial class LevelState : State
    {
        [Export]
        public PackedScene LevelOverride;

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
            // Pull the initial level from the message
            var levelScene = (PackedScene)message.GetValueOrDefault(Consts.LEVEL_KEY);

            // Ready scene
            _Level = (LevelOverride ?? levelScene).Instantiate<Level>();
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

