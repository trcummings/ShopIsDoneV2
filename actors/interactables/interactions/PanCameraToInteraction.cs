using System;
using Godot;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Interactables.Interactions
{
    [Tool]
    public partial class PanCameraToInteraction : Interaction
    {
        [Export]
        private Node3D _Target;

        [Inject]
        private CameraService _CameraSystem;

        public override void Execute()
        {
            // Inject
            InjectionProvider.Inject(this);

            // Run pan camera command
            var command = _CameraSystem.WaitForCameraToReachTarget(_Target);
            command.Connect(nameof(command.Finished), Callable.From(Finish), (uint)ConnectFlags.OneShot);
            command.Execute();
        }

        public override string[] _GetConfigurationWarnings()
        {
            if (_Target == null) return new string[] { "Requires camera target!" };
            return base._GetConfigurationWarnings();
        }
    }
}

