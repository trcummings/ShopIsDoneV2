using Godot;
using System;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;
using ShopIsDone.Utils;

namespace ShopIsDone.Models.IsometricModels
{
    [Tool]
    public partial class IsometricModel : Node3D, IModel, IIsometricViewable
    {
        [Signal]
        public delegate void AnimationFinishedEventHandler(string animName);

        [Signal]
        public delegate void AnimationEventFiredEventHandler(string eventName);

        // Nodes
        [Export]
        protected IsometricSprite2D _Sprite;

        [ExportGroup("Animation Mapping")]
        [Export]
        private bool _ForceAnimLowercase = false;

        // This maps a normalized action name that we call in a state handler or
        // through a script to a specific animation name that the model may have
        // for that action
        [Export]
        private Dictionary<string, string> _AnimationNameMap = new Dictionary<string, string>();

        // State vars
        protected Vector3 _ViewedDir = Vec3.BackRight;
        protected Vector3 _FacingDir = Vector3.Left;

        // Action vars
        protected string _ActionName = "";

        public override void _Ready()
        {
            base._Ready();
            _Sprite.AnimationEventFired += FireEvent;
        }

        public void Init()
        {

        }

        private void FireEvent(string eventName)
        {
            EmitSignal(nameof(AnimationEventFired), eventName);
        }

        public virtual string GetDefaultAnimationName()
        {
            return "RESET";
        }

        public void SetFacingDir(Vector3 facingDir)
        {
            if (_FacingDir != facingDir)
            {
                _FacingDir = facingDir;
                Task _ = RunIsometricAnimation();
            }
        }

        public void SetViewedDir(Vector3 viewedDir)
        {
            if (_ViewedDir != viewedDir)
            {
                _ViewedDir = viewedDir;
                Task _ = RunIsometricAnimation();
            }
        }

        private string TransformAnimName(string rawActionName)
        {
            var animationName = rawActionName;

            // Map the given 
            if (_AnimationNameMap.ContainsKey(rawActionName))
            {
                animationName = _AnimationNameMap[rawActionName];
            }

            return _ForceAnimLowercase ? animationName.ToLower() : animationName;
        }

        public virtual async Task PerformAnimation(string animName, bool advance = false)
        {
            // Persist action name in state
            _ActionName = TransformAnimName(animName);

            // Await action to finish
            await RunIsometricAnimation();

            // Emit signal
            EmitSignal(nameof(AnimationFinished), _ActionName);
        }

        public void PauseAnimation()
        {
            _Sprite.PauseAnimation();
        }

        public void UnpauseAnimation()
        {
            _Sprite.UnpauseAnimation();
        }

        private async Task SetSpriteAction(string actionName)
        {
            // Do not set the action if we don't yet have one (or if the sprite isn't set yet)
            if (string.IsNullOrEmpty(actionName) || _Sprite == null)
            {
                await ToSignal(GetTree(), "process_frame");
                return;
            }

            // Play animation
            _Sprite.PlayAnimation(actionName);
            await ToSignal(_Sprite, nameof(_Sprite.AnimationFinished));
        }

        private Task RunIsometricAnimation()
        {
            // Get animation direction
            var dir = Vec3.TransformFacingDir(_FacingDir, _ViewedDir);

            // Update the sprite's variables
            _Sprite?.SetDirection(Vec3.DirToString(dir).ToLower());

            // Run the animation
            return SetSpriteAction(_ActionName);
        }
    }

}

