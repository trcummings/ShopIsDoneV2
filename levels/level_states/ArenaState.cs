using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Arenas;
using ShopIsDone.Actors;

namespace ShopIsDone.Levels.States
{
    // This state initializes an arena
    public partial class ArenaState : State
    {
        [Export]
        private Haskell _Haskell;

        private Arena _Arena;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Null out actor movement with dummy input
            _Haskell.Init(new ActorInput());

            // Pull arena from message
            _Arena = (Arena)message.GetValueOrDefault(Consts.States.ARENA_KEY, default);
            // Initialize arena
            _Arena.Init(_Haskell);

            // Finish start hook
            base.OnStart(message);
        }

        public override void OnExit(string nextState)
        {
            // Clean up arena
            _Arena.CleanUp();
            
            base.OnExit(nextState);
        }
    }
}

