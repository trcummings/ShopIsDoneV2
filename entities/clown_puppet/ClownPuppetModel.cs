using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using ShopIsDone.Models;

namespace ShopIsDone.Entities.ClownPuppet
{
    [Tool]
    public partial class ClownPuppetModel : Model
    {
        [Export]
        public Material DissolveMaterial;

        [Export]
        private Skeleton3D _Skeleton;

        [Export]
        private AnimationTree _AnimTree;

        private Callable _SetHandBlend;
        private Callable _SetWarpAmount;

        public override void _Ready()
        {
            // Get the Skeleton IK nodes and start them
            foreach (var ik in _Skeleton.GetChildren().OfType<SkeletonIK3D>())
            {
                ik.Start();
            }

            // Set callables
            _SetHandBlend = Callable.From<float>(SetHandBlend);
            _SetWarpAmount = Callable.From<float>(SetWarpAmount);
        }

        public override async Task PerformAnimation(string actionName)
        {
            if (actionName == Consts.Anims.ClownPuppet.RAISE_HAND)
            {
                // Tween the Blend Hand Parameter in the animation tree to .99
                await ToSignal(GetTree().CreateTween().TweenMethod(_SetHandBlend, 0, 0.99, 1.5), "finished");
            }

            else if (actionName == Consts.Anims.ClownPuppet.LOWER_HAND)
            {
                // Tween the Blend Hand Parameter in the animation tree back down to 0
                await ToSignal(GetTree().CreateTween().TweenMethod(_SetHandBlend, 0.99, 0, 1.5), "finished");
            }

            else if (actionName == Consts.Anims.ClownPuppet.WARP_AWAY)
            {
                // Tween the warp material from min value to maximum value
                await ToSignal(GetTree().CreateTween().TweenMethod(_SetWarpAmount, 0, 1, 1.5), "finished");
            }

            else if (actionName == Consts.Anims.ClownPuppet.WARP_IN)
            {
                // Tween the warp material from max value to minimum value
                await ToSignal(GetTree().CreateTween().TweenMethod(_SetWarpAmount, 1, 0, 1.5), "finished");
            }

            // Otherwise, send it to the base handler
            else await base.PerformAnimation(actionName);

            // Emit signal
            EmitSignal(nameof(AnimationFinished), actionName);
        }

        private void SetHandBlend(float blendAmount)
        {
            _AnimTree.Set("parameters/blend_hand/blend_amount", blendAmount);
        }

        private void SetWarpAmount(float warpAmount)
        {
            (DissolveMaterial as ShaderMaterial).SetShaderParameter("distortion", warpAmount);
        }
    }
}

