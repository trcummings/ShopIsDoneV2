using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Arenas;
using ShopIsDone.Actors;

namespace ShopIsDone.Levels.States
{
    public partial class ArenaState : State
    {
        [Export]
        private Haskell _Haskell;

        private Arena _Arena;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Null out actor movement with dummy input
            _Haskell.Init(new ActorInput());

            // Pull arena state from message
            var area = (EnterArenaArea)message.GetValueOrDefault(Consts.States.ENTER_ARENA_AREA_KEY, default);
            // Disable the area
            area.CallDeferred(nameof(area.Disable));

            // Pull arena from message
            _Arena = (Arena)message.GetValueOrDefault(Consts.States.ARENA_KEY, default);
            // Initialize arena
            _Arena.Init();

            // TODO: Move units into arena

            // Finish start hook
            base.OnStart(message);
        }
    }
}

