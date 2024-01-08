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
            AnimationEventFired += OnEvent;
        }

        private void OnEvent(string eventName)
        {
            if (eventName == "footstep") EmitSignal(nameof(Footstep));
        }
    }
}

