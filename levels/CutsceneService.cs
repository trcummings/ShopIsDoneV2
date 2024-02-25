using Godot;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using System;

namespace ShopIsDone.Levels
{
    public partial class CutsceneService : Node, IService
    {
        [Signal]
        public delegate void CutsceneBeganEventHandler();

        [Signal]
        public delegate void CutsceneFinishedEventHandler();

        [Export]
        private StateMachine _LevelStateMachine;

        public void StartCutscene(Dictionary<string, Variant> message = null)
        {
            _LevelStateMachine.ChangeState(Consts.States.CUTSCENE, message);
            EmitSignal(nameof(CutsceneBegan));
        }

        public void FinishCutscene()
        {
            // Defer finishing cutscene for a short pause
            GetTree().CreateTimer(0.05f).Connect(
                "timeout",
                Callable.From(() => EmitSignal(nameof(CutsceneFinished))),
                (uint)ConnectFlags.OneShot
            );
        }
    }
}

