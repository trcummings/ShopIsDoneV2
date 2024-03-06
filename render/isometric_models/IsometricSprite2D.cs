using Godot;
using System;
using System.Linq;
using Godot.Collections;

namespace ShopIsDone.Models.IsometricModels
{
    [Tool]
    public partial class IsometricSprite2D : Node2D
    {
        [Signal]
        public delegate void AnimationFinishedEventHandler(string animName);

        [Signal]
        public delegate void AnimationPausedEventHandler();

        [Signal]
        public delegate void AnimationUnpausedEventHandler();

        [Signal]
        public delegate void AnimationEventFiredEventHandler(string eventName);

        [Export]
        protected Array<string> LoopingAnimations;

        [Export]
        protected AnimationPlayer _AnimPlayer;

        private string _PrevAnimation;

        public override void _Ready()
        {
            base._Ready();
            // Connect to animation finished
            _AnimPlayer.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));
        }

        public bool IsLoopingAnimation(string animName)
        {
            return LoopingAnimations?.Any(lAnim => lAnim.ToLower() == animName.ToLower()) ?? false;
        }

        public bool HasAnimation(string animName)
        {
            return _AnimPlayer.HasAnimation(animName);
        }

        public async virtual void PlayAnimation(string animName)
        {
            // Main animation player
            if (
                _AnimPlayer.HasAnimation(animName) &&
                !string.IsNullOrEmpty(animName) &&
                _AnimPlayer.CurrentAnimation != animName &&
                // HACK: This stops non-looping animations from playing over and
                // over again each time the view direction changes. However, if
                // we want to play the same animation over and over again, and
                // it's not in the looping animations, this will preclude that.
                // An alternate approach is to simply define a new animation that
                // can be safely looped in every view direction and add it to
                // whatever state the entity will be in
                (LoopingAnimations.Contains(animName) || animName != _PrevAnimation)
            )
            {
                _PrevAnimation = animName;
                _AnimPlayer.Play(animName);
            }
            else
            {
                await ToSignal(GetTree(), "process_frame");
                EmitSignal(nameof(AnimationFinished));
            }
        }

        public void PauseAnimation()
        {
            if (_AnimPlayer.IsPlaying())
            {
                _AnimPlayer.Stop(false);
                EmitSignal(nameof(AnimationPaused));
            }
        }

        public void UnpauseAnimation()
        {
            if (_AnimPlayer.CurrentAnimation != "")
            {
                _AnimPlayer.Play();
                EmitSignal(nameof(AnimationUnpaused));
            }
        }

        public virtual void SetDirection(string direction)
        {
            // Set direction here
        }

        private void FireEvent(string eventName)
        {
            EmitSignal(nameof(AnimationEventFired), eventName);
        }

        private void OnAnimationFinished(string animName)
        {
            // If animation should loop, then run it again
            if (IsLoopingAnimation(animName)) PlayAnimation(animName);
            EmitSignal(nameof(AnimationFinished), animName);
        }
    }

}

