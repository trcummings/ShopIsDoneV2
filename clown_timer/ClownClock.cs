using Godot;
using System;

namespace ShopIsDone.Arenas.ClownTimer
{
	public partial class ClownClock : Node2D, IClownTimerTick
    {
		private Sprite2D _ClockHand;

		public override void _Ready()
		{
			_ClockHand = GetNode<Sprite2D>("%ClownClockHand");
        }

        public void StartClownTimer()
        {
            // Do nothing
        }

        public void StopClownTimer()
        {
            // Do nothing
        }

        public void ClownTimerTick(double delta)
		{
			_ClockHand.Rotate(Mathf.DegToRad((float)delta * 25));
		}
	}
}
