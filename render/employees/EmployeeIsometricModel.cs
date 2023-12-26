using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Models.IsometricModels;

namespace ShopIsDone.Models.Employees
{
    [Tool]
	public partial class EmployeeIsometricModel : IsometricModel
	{
        [Signal]
        public delegate void FootstepEventHandler();

        public override void _Ready()
        {
            base._Ready();
            AnimationEventFired += (string eventName) =>
            {
                if (eventName == "footstep") EmitSignal(nameof(Footstep));
            };
        }

        public override async Task PerformAnimation(string rawAnimName, bool advance = false)
        {
            var actionName = rawAnimName;

            // Default to "idle"
            if (actionName == Consts.Anims.DEFAULT) actionName = "idle";
            // Catch take hit
            else if (actionName == Consts.Anims.TAKE_HIT) actionName = "take_hit";
            // Catch evade
            else if (actionName == "Evade") actionName = "alert";
            // Catch do task
            else if (actionName == Consts.Anims.Employee.DO_TASK) actionName = "do_task";

            await base.PerformAnimation(actionName.ToLower(), advance);
        }
    }
}

