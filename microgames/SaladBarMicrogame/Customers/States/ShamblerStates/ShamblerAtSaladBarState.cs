using System;
using Godot;

namespace ShopIsDone.Microgames.SaladBar.States
{
    public partial class ShamblerAtSaladBarState : AtSaladBarState
	{
        [Signal]
        public delegate void TookSlapEventHandler();

        [Signal]
        public delegate void TookDamageEventHandler();


        [Export]
        public int HandsPerWave = 4;

        [Export]
        public int MaxDamage = 3;

        private int _Damage = 0;

        protected override bool ShouldLeave()
        {
            return _Damage >= MaxDamage;
        }

        protected override void GatherActions()
        {
            for (int i = 0; i < HandsPerWave; i++)
            {
                _Actions.Enqueue(() => { Events.EmitSignal(nameof(Events.ShamblerHandRequested)); });
            }
        }

        protected async override void OnSlapped(Grabber grabber)
        {
            if (grabber is ShamblerHand hand)
            {
                if (hand.NumSlaps >= 3)
                {
                    hand.Withdraw();
                    _Damage += 1;
                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                    EmitSignal(nameof(TookDamage));
                }
                else
                {
                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                    EmitSignal(nameof(TookSlap));
                }
            } 
        }
    }
}

