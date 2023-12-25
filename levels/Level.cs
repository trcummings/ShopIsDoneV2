using Godot;
using System;
using ShopIsDone.Arenas;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Levels
{
    public partial class Level : Node
    {
        [Export]
        private StateMachine _LevelStateMachine;

        public override void _Ready()
        {
            SetProcess(false);

            // Wire up arena entrances
            var entrances = GetTree().GetNodesInGroup("arena_entrance").OfType<EnterArenaArea>();
            foreach (var entrance in entrances)
            {
                entrance.EnteredArena += (arena) => OnPlayerEnteredArena(entrance, arena);
            }
        }

        public void Init()
        {
            // Change state to initializing state
            _LevelStateMachine.ChangeState("InitializingState");
        }

        private void OnPlayerEnteredArena(EnterArenaArea area, Arena arena)
        {
            _LevelStateMachine.ChangeState("ArenaState", new Dictionary<string, Variant>()
            {
                { Consts.States.ARENA_KEY, arena },
                { Consts.States.ENTER_ARENA_AREA_KEY, area }
            });
        }
    }
}

