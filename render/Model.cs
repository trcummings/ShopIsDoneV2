﻿using System;
using Godot;
using System.Threading.Tasks;
using Godot.Collections;
using System.Linq;

namespace ShopIsDone.Models
{
    [Tool]
    public partial class Model : Node3D, IModel
    {
        [Signal]
        public delegate void AnimationFinishedEventHandler(string animName);

        [Signal]
        public delegate void AnimationEventFiredEventHandler(string eventName);

        [Export]
        public Array<string> LoopingAnimations;

        [Export]
        protected AnimationPlayer _AnimPlayer;

        public override void _Ready()
        {
            // Connect to animation finished
            _AnimPlayer?.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));
        }

        private bool IsLoopingAnimation(string animName)
        {
            return LoopingAnimations?.Any(lAnim => lAnim.ToLower() == animName.ToLower()) ?? false;
        }

        public virtual void Init()
        {
            // Do stuff here
        }

        public virtual string GetDefaultAnimationName()
        {
            return "RESET";
        }

        public virtual void SetFacingDir(Vector3 facingDir)
        {
            // Rotate the model
            LookAt(GlobalPosition - facingDir, Vector3.Up);
        }

        private void FireEvent(string eventName)
        {
            EmitSignal(nameof(AnimationEventFired), eventName);
        }

        public virtual async Task PerformAnimation(string animName, bool advance = false)
        {
            // Save ourselves some output here, if it's the default call and we
            // don't have any animations, ignore it
            if (
                _AnimPlayer == null || (
                    animName == GetDefaultAnimationName() &&
                    _AnimPlayer?.GetAnimationList().Count() == 0)
                )
            {
                // Await an idle frame
                await ToSignal(GetTree(), "process_frame");
            }
            // Then, filter actions through the main animation player
            else if (_AnimPlayer.HasAnimation(animName))
            {
                // Play normally
                _AnimPlayer.Play(animName);

                // If no advance, wait for it to finish
                if (!advance) await ToSignal(_AnimPlayer, "animation_finished");
                // Otherwise, advance the animation position to the end
                else
                {
                    _AnimPlayer.Advance(_AnimPlayer.CurrentAnimationLength);
                    await ToSignal(GetTree(), "process_frame");
                }
            }
            // Otherwise, error case
            else
            {
                GD.PrintErr($"Action {animName} not valid for {Name}");
                // Await an idle frame
                await ToSignal(GetTree(), "process_frame");
            }

            // Emit signal
            EmitSignal(nameof(AnimationFinished));
        }

        public void PauseAnimation()
        {
            if (_AnimPlayer.IsPlaying()) _AnimPlayer.Stop(false);
        }

        public void UnpauseAnimation()
        {
            if (_AnimPlayer.CurrentAnimation != "") _AnimPlayer.Play();
        }

        private void OnAnimationFinished(string animName)
        {
            // If animation should loop, then run it again
            if (IsLoopingAnimation(animName)) _ = PerformAnimation(animName);
            EmitSignal(nameof(AnimationFinished), animName);
        }
    }
}

