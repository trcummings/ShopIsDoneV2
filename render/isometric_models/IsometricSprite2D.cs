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

        [Export]
        protected Array<string> LoopingAnimations;

        [Export]
        protected AnimationPlayer _AnimPlayer;

        public override void _Ready()
        {
            // Connect to animation finished
            _AnimPlayer.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));
        }

        public bool IsLoopingAnimation(string animName)
        {
            return LoopingAnimations?.Any(lAnim => lAnim.ToLower() == animName.ToLower()) ?? false;
        }

        public async virtual void PlayAnimation(string animName)
        {
            // Main animation player
            if (_AnimPlayer.HasAnimation(animName) && _AnimPlayer.CurrentAnimation != animName)
            {
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

        private void OnAnimationFinished(string animName)
        {
            // If animation should loop, then run it again
            if (IsLoopingAnimation(animName)) PlayAnimation(animName);
            EmitSignal(nameof(AnimationFinished), animName);
        }
    }

}

