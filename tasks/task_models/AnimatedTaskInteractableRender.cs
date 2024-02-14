using Godot;

namespace ShopIsDone.Tasks.TaskModels
{
    public partial class AnimatedTaskInteractableRender : TaskModelHelper
    {
        [Export]
        private AnimationPlayer _AnimPlayer;

        [Export]
        public string AnimName;

        public override void _Ready()
        {
            // Advance the animation to the initial frame
            _AnimPlayer.Play(AnimName);
            _AnimPlayer.Advance(0);
            _AnimPlayer.Stop(false);
        }

        public override void Init(int current, int total, float percent)
        {
            // Get animation
            var anim = _AnimPlayer.GetAnimation(AnimName);

            // Calculate amount to advance by based on percentage
            var progressPercent = 1f - (current / (float)total);
            var advanceTime = progressPercent * anim.Length;

            // Advance the animation to percentage amount
            _AnimPlayer.Seek(advanceTime, true);
        }

        public override void SetProgress(int amount, int current, int total, float percent)
        {
            // Get animation
            var anim = _AnimPlayer.GetAnimation(AnimName);

            // Calculate amount to advance by based on percentage
            var progressPercent = 1f - (current / (float)total);
            var advanceTime = progressPercent * anim.Length;

            // Advance the animation to percentage amount
            _AnimPlayer.Seek(advanceTime, true);
        }
    }
}

