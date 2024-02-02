using Godot;
using System;

namespace ShopIsDone.Arenas.ClownTimer
{
	public partial class ClownClock : Node2D
	{
		private Sprite2D _ClockHand;

		public override void _Ready()
		{
			_ClockHand = GetNode<Sprite2D>("%ClownClockHand");
        }

		public void TickClock(float delta)
		{
			_ClockHand.Rotate(Mathf.DegToRad(delta * 25));
		}
	}
}
