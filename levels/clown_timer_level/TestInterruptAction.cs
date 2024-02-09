using Godot;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using System;

namespace ShopIsDone.Levels.ClownTimerLevel
{
	public partial class TestInterruptAction : ArenaAction
    {
        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return new SeriesCommand(
                base.Execute(message),
                new WaitForCommand(Entity, 3f)
            );
        }
    }
}