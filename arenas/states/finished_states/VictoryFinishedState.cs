using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils.UI;

namespace ShopIsDone.Arenas.States.Finished
{
    public partial class VictoryFinishedState : State
    {
        [Export]
        private Arena _Arena;

        [Export]
        private ControlTweener _Tweener;

        public async override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Victory UI
            await _Tweener.TweenInAsync(0.5f);
            await ToSignal(GetTree().CreateTimer(2f), "timeout");
            await _Tweener.TweenOutAsync(0.5f);

            _Arena.FinishArena();
        }
    }
}