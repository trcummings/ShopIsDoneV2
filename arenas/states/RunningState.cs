using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;

namespace ShopIsDone.Arenas.States
{
	public partial class RunningState : State
	{
		[Inject]
		private CameraService _CameraService;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
            // Dependency injection
            InjectionProvider.Inject(this);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Handle camera rotation
            if (Input.IsActionJustPressed("rotate_camera_left"))
            {
                _CameraService.RotateLeft();
            }
            if (Input.IsActionJustPressed("rotate_camera_right"))
            {
                _CameraService.RotateRight();
            }
        }
    }
}
