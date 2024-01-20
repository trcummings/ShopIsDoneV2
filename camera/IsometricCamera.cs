using Godot;
using System;
using System.Linq;
using ShopIsDone.Utils;
using ShopIsDone.Cameras.Zooms;

namespace ShopIsDone.Cameras
{
    public partial class IsometricCamera : Node3D
    {
        [Signal]
        public delegate void CameraRotatedEventHandler(Vector3 newBasis);

        [Signal]
        public delegate void CameraReachedTargetEventHandler();

        [Signal]
        public delegate void ZoomChangedEventHandler(float zoom);

        [Export]
        private CameraZoom _Zoom;

        // Rotation Consts
        public const float ROTATION_SPEED = 6.0F;

        // Translation Consts
        public const float MOVEMENT_DURATION = 0.25F;
        public const Tween.EaseType EASE_TYPE = Tween.EaseType.OutIn;
        public const Tween.TransitionType TRANS_TYPE = Tween.TransitionType.Linear;

        // Camera state
        private float _Yaw = -45.0F; // Y-Axis rotation

        // Targeting state
        public Node3D Target = null;
        private Vector3 _TargetDestination;

        // Movement State
        private Tween _MoveTween;
        public float MoveDuration = MOVEMENT_DURATION;

        public Camera3D Camera { get { return _Camera; } }
        private Camera3D _Camera;
        private Vector3 _PrevForwardBasis;

        public override void _Ready()
        {
            SetProcess(false);

            // Ready nodes
            _Camera = GetNode<Camera3D>("%Camera");

            // Record previous forward basis
            _PrevForwardBasis = GetNormalizedForwardBasis();

            // Set current target destination to initial position
            _TargetDestination = GlobalPosition;

            // initially hard-set rotation
            GlobalRotation = new Vector3(
                // Set the pitch to -30 degrees
                Mathf.DegToRad(-30f),
                // Force the camera to the Yaw of -45deg
                Mathf.DegToRad(_Yaw),
                0
            );

            // Connect to zoom
            _Zoom.ZoomChanged += (float newZoom) =>
            {
                EmitSignal(nameof(ZoomChanged), newZoom);
            };
        }

        public override void _PhysicsProcess(double delta)
        {
            // Handle rotation and movement
            RotateAround(delta);
            UpdateTargetReached(delta);

            // Update Zoom
            _Zoom.UpdateZoom(delta);

            // Emit any changes to our basis
            var newForwardBasis = GetNormalizedForwardBasis();
            if (newForwardBasis != _PrevForwardBasis)
            {
                EmitSignal(nameof(CameraRotated), newForwardBasis);
            }

            // Track the new basis
            _PrevForwardBasis = newForwardBasis;

            // Propagate properties to camera clones
            foreach (var clone in GetTree().GetNodesInGroup("camera_clone").OfType<Camera3D>())
            {
                clone.HOffset = _Camera.HOffset;
                clone.VOffset = _Camera.VOffset;
                _Zoom.SyncClone(clone);
            }

            // Set global camera zoom shader
            RenderingServer.GlobalShaderParameterSet("camera_zoom", _Zoom.ZoomAmount);
        }

        public void SetOffset(Vector2 offset)
        {
            _Camera.HOffset = offset.X;
            _Camera.VOffset = offset.Y;
        }

        public void MakeCurrent()
        {
            _Camera.MakeCurrent();
        }

        public (Vector3, Vector3) GetCameraVectors()
        {
            // Get camera forward vector from basis with a vertical 0 component
            var forward = -_Camera.GlobalTransform.Basis.Z with { Y = 0 };

            // Get the camera's right vector from the basis
            var right = GlobalTransform.Basis.X;

            // Return as tuple
            return (forward, right);
        }

        public static Vector3 GetXformedDir((Vector3, Vector3) vectors, Vector2 input, float rotateBy = Mathf.Pi / 4)
        {
            // Pull out vectors
            var (forward, right) = vectors;

            // Calculate movement vectors normalized to the forward and right vector
            var moveVec = (right * input.X) + (forward * input.Y);

            // Rotate movement vector around the camera's offset (defaulted to 45 deg)
            return moveVec.Rotated(Vector3.Up, rotateBy).Normalized() with
            {
                Y = 0
            };
        }

        public Vector3 GetBasisXformedDir(float horizontal, float vertical, float rotateBy = Mathf.Pi / 4)
        {
            return GetXformedDir(GetCameraVectors(), new Vector2(horizontal, vertical), rotateBy);
        }

        public Vector3 GetNormalizedForwardBasis()
        {
            // Get camera forward vector from basis and set its vertical component to 0
            var forward = -_Camera.GlobalTransform.Basis.Z with { Y = 0 };

            return new Vector3(Mathf.Sign(forward.X), 0.0F, Mathf.Sign(forward.Z));
        }

        public void RotateRight()
        {
            _Yaw += 90.0F;
        }

        public void RotateLeft()
        {
            _Yaw -= 90.0F;
        }

        public void ZoomIn()
        {
            _Zoom.ZoomIn();
        }

        public void ZoomOut()
        {
            _Zoom.ZoomOut();
        }

        public float GetZoom()
        {
            return _Zoom.GetZoom();
        }

        public void SetZoom(float zoomAmount)
        {
            _Zoom.SetZoom(zoomAmount);
        }

        public void ForceZoom(float zoomAmount)
        {
            _Zoom.ForceZoom(zoomAmount);
        }

        public void SetYawToDir(Vector3 dir)
        {
            // Set yaw
            // NB: Offset all rotation by 45 degrees
            _Yaw = Vec3.FacingDirToYDeg(dir) - 45;
        }

        private void RotateAround(double delta)
        {
            // Lerp to the appropriate rotation and wrap the value around Tau
            var newY = Mathf.PosMod(Mathf.LerpAngle(Rotation.Y, Mathf.DegToRad(_Yaw), delta * ROTATION_SPEED), Mathf.Tau);

            // Set rotation
            Rotation = Rotation with { Y = (float)newY };
        }

        private void SetTargetDestination(Vector3 destination)
        {
            // Return early if it's a redundant update
            if (_TargetDestination == destination) return;

            // Otherwise, set it
            _TargetDestination = destination;

            // Clear current tweens and create new ones
            if (_MoveTween != null)
            {
                _MoveTween.Kill();
                _MoveTween = null;
            }

            // When we set a target, we want to set also our initial position and
            // fire off a tween to that target
            _MoveTween = GetTree().CreateTween();
            _MoveTween
                .TweenProperty(this, "global_position", _TargetDestination, MoveDuration)
                // Set ease and trans type
                .SetEase(Tween.EaseType.OutIn)
                .SetTrans(Tween.TransitionType.Linear);
        }

        private void UpdateTargetReached(double _)
        {
            // If no target, emit and ignore
            if (Target == null)
            {
                EmitSignal(nameof(CameraReachedTarget));
                return;
            }

            // If target is visible but we have no tween for it, run the set
            // target function again and start tweening immediately
            if (IsTargetVisible())
            {
                SetTargetDestination(Target.GlobalPosition);
            }
            // If it's not, return early and emit
            else
            {
                EmitSignal(nameof(CameraReachedTarget));
                return;
            }

            // If we're at our target's position, emit a signal
            if (GlobalPosition == _TargetDestination)
            {
                EmitSignal(nameof(CameraReachedTarget));
            }
        }

        private bool IsTargetVisible()
        {
            //// If it doesn't have a "IsLit" method, assume that it's visible
            //if (!Target.HasMethod("IsLit"))
            //{
            //    // Unless it's an entity with a light detector component
            //    if (Target is LevelEntity entity && entity.HasComponent<LightDetectorComponent>())
            //    {
            //        return entity.GetComponent<LightDetectorComponent>().IsLit();
            //    }

            //    // Otherwise, assume it's visible
            //    return true;
            //}
            //// But if it does, use that to judge
            //return (bool)Target.Call("IsLit");
            return true;
        }
    }
}