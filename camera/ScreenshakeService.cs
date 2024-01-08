using System;
using Godot;
using ShopIsDone.Utils.DependencyInjection;
using static ShopIsDone.Cameras.ScreenshakeHandler;

namespace ShopIsDone.Cameras
{
	// Convenience service to inject into lower nodes that need to access
	// screenshake functionality
	public partial class ScreenshakeService : Node, IService
    {
		[Export]
		public IsometricCamera _IsometricCamera;

		[Export]
		public ScreenshakeHandler _Screenshake;

        public override void _Ready()
        {
			_Screenshake.ShakeOffsetUpdated += _IsometricCamera.SetOffset;
        }

        public void Shake(ShakePayload payload)
		{
			_Screenshake.Shake(payload);
		}

        public void Shake(
			ShakePayload.ShakeSizes size = ShakePayload.ShakeSizes.Mild,
			ShakeAxis axis = ShakeAxis.Both
		)
        {
            Shake(new ShakePayload(size) { Axis = axis });
        }

        public void StopShake()
		{
			_Screenshake.HaltShake();
        }
	}
}

