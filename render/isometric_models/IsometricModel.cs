using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopIsDone.Utils;

namespace ShopIsDone.Models.IsometricModels
{
    public partial class IsometricModel : Node3D, IModel, IIsometricViewable
    {
        [Signal]
        public delegate void AnimationFinishedEventHandler(string animName);

        // Nodes
        [Export]
        protected IsometricSpriteRenderer2D _Sprite;

        // State vars
        protected Vector3 _ViewedDir = Vec3.BackRight;
        protected Vector3 _FacingDir = Vector3.Left;

        // Action vars
        protected string _ActionName = "";

        public void Init()
        {

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

        public virtual async Task PerformAnimation(string animName, bool advance = false)
        {
            // Persist action name in state
            _ActionName = animName;

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
                await ToSignal(GetTree(), "idle_frame");
                return;
            }

            // Play animation
            _Sprite.PlayAnimation(actionName);
            await ToSignal(_Sprite, nameof(IsometricSpriteRenderer2D.AnimationFinished));
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

