using System;
using Godot;
using System.Linq;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Models.IsometricModels;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Cameras
{
	public partial class CameraService : Node, IService
    {
        [Export]
        public IsometricCamera _IsometricCamera;

        private Callable _CameraRotatedCallable;
        private Callable _IsoSpriteAddedCallable;

        public override void _Ready()
        {
            base._Ready();
            _CameraRotatedCallable = new Callable(this, nameof(OnCameraRotated));
            _IsoSpriteAddedCallable = new Callable(this, nameof(OnPotentialIsoSpriteAdded));
        }

        public void Init()
        {
            // Connect to camera rotation event
            _IsometricCamera.SafeConnect(nameof(_IsometricCamera.CameraRotated), _CameraRotatedCallable);

            // Initially send out camera rotation event to set all sprites correctly
            OnCameraRotated(_IsometricCamera.GetNormalizedForwardBasis());

            // When an isometric sprite gets added, make sure it gets initialized
            // with the proper viewing direction
            GetTree().SafeConnect("node_added", _IsoSpriteAddedCallable);
        }

        public Vector3 GetBasisXformedDir(float horizontal, float vertical, float rotateBy = Mathf.Pi / 4)
        {
            return _IsometricCamera.GetBasisXformedDir(horizontal, vertical, rotateBy);
        }

        public void RotateRight()
        {
            _IsometricCamera.RotateRight();
        }

        public void RotateLeft()
        {
            _IsometricCamera.RotateLeft();
        }

        public Command SetCameraTarget(Node3D target)
        {
            return new SetCameraTargetCommand()
            {
                Camera = _IsometricCamera,
                Target = target
            };
        }

        public Node3D GetCameraTarget()
        {
            return _IsometricCamera.Target;
        }

        public Command WaitForCameraToReachTarget(Node3D target)
        {
            return new WaitForCameraToReachTargetCommand()
            {
                Camera = _IsometricCamera,
                Target = target
            };
        }

        public Command ZoomCameraTo(float zoomAmount)
        {
            return new WaitForCameraToZoomCommand()
            {
                Camera = _IsometricCamera,
                ZoomTarget = zoomAmount
            };
        }

        public void RotateCameraTo(Vector3 facingDir)
        {
            _IsometricCamera.SetYawToDir(facingDir);
        }

        public Command RunRotateCameraTo(Vector3 facingDir, Command next)
        {
            return new SeriesCommand(
                new ActionCommand(() => RotateCameraTo(facingDir)),
                new WaitForCommand(this, 0.15f),
                next
            );
        }

        // Decorator command that saves whatever the prior target was and goes
        // back to it after a command is completed
        public Command PanToTemporaryCameraTarget(Node3D target, Command next)
        {
            Node3D oldCameraTarget = null;
            return new SeriesCommand(
                // Preserve old camera target
                new ActionCommand(() =>
                {
                    oldCameraTarget = GetCameraTarget();
                }),
                SetCameraTarget(target),
                new DeferredCommand(() => new SeriesCommand(
                    next,
                    // Set camera back to old target
                    WaitForCameraToReachTarget(oldCameraTarget)
                ))
            );
        }

        // Decorator command that zooms in and out after completing a given command
        public Command TemporaryCameraZoom(Command next)
        {
            return new SeriesCommand(
                // Punch in
                ZoomCameraTo(0.5f),
                // Run deferred action
                new DeferredCommand(() => next),
                // Zoom out
                ZoomCameraTo(1)
            );
        }

        public void WarpCameraTo(Vector3 position)
        {
            _IsometricCamera.GlobalPosition = position;
        }

        public void SetCameraMoveDuration(float newDuration)
        {
            _IsometricCamera.MoveDuration = newDuration;
        }

        public void ResetCameraMoveDuration()
        {
            _IsometricCamera.MoveDuration = IsometricCamera.MOVEMENT_DURATION;
        }

        private const string ISOMETRIC_SPRITE_GROUP = "isometric_sprite";

        // Camera rotation + Iso Sprite updates
        private void OnCameraRotated(Vector3 newBasis)
        {
            // Get every node tagged with "isometric_sprite" and set its viewed direction
            var isometricSprites = GetTree().GetNodesInGroup(ISOMETRIC_SPRITE_GROUP).OfType<IIsometricViewable>();
            foreach (var sprite in isometricSprites) sprite.SetViewedDir(newBasis);
        }

        private void OnPotentialIsoSpriteAdded(Node node)
        {
            if (node.IsInGroup(ISOMETRIC_SPRITE_GROUP) && node is IIsometricViewable sprite)
            {
                sprite.SetViewedDir(_IsometricCamera.GetNormalizedForwardBasis());
            }
        }

        // Camera Commands
        private partial class CameraCommand : Command
        {
            public IsometricCamera Camera;
        }

        private partial class SetCameraTargetCommand : CameraCommand
        {
            public Node3D Target;

            public override void Execute()
            {
                // Set camera target
                Camera.Target = Target;

                Finish();
            }
        }

        private partial class WaitForCameraToReachTargetCommand : CameraCommand
        {
            public Node3D Target;

            public async override void Execute()
            {
                // If we have a target, set camera target and wait for it to reach the target
                if (Target != null)
                {
                    Camera.Target = Target;
                    await ToSignal(Camera, nameof(IsometricCamera.CameraReachedTarget));
                }

                Finish();
            }
        }

        private partial class WaitForCameraToZoomCommand : CameraCommand
        {
            public float ZoomTarget;

            public override void Execute()
            {
                // Connect to isometric camera zoom event
                Camera.ZoomChanged += OnZoomChanged;

                // Set camera zoom target
                Camera.SetZoom(ZoomTarget);
            }

            private void OnZoomChanged(float newZoom)
            {
                // Once we reach the zoom target
                if (Mathf.Snapped(newZoom, 0.1f) == Mathf.Snapped(ZoomTarget, 0.1f)) Finish();
            }
        }
    }
}

