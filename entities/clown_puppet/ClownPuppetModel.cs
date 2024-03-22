using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using ShopIsDone.Models;
using ModelConsts = ShopIsDone.Models.Consts;

namespace ShopIsDone.Entities.ClownPuppet
{
    [Tool]
    public partial class ClownPuppetModel : Model
    {
        [Signal]
        public delegate void ClatteredEventHandler();

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
            base._Ready();
            AnimationEventFired += OnEvent;

            // Get the Skeleton IK nodes and start them
            foreach (var ik in _Skeleton.GetChildren().OfType<SkeletonIK3D>())
            {
                ik.Start();
            }

            // Set callables
            _SetHandBlend = new Callable(this, nameof(SetHandBlend));
            _SetWarpAmount = new Callable(this, nameof(SetWarpAmount));
        }

        public override async Task PerformAnimation(string rawActionName)
        {
            var actionName = TransformAnimName(rawActionName);

            if (actionName == ModelConsts.Anims.ClownPuppet.RAISE_HAND)
            {
                FireEvent("clatter");
                // Tween the Blend Hand Parameter in the animation tree to .99
                await ToSignal(GetTree().CreateTween().BindNode(this).TweenMethod(_SetHandBlend, 0f, 1f, 1.5f), "finished");
            }

            else if (actionName == ModelConsts.Anims.ClownPuppet.LOWER_HAND)
            {
                FireEvent("clatter");
                // Tween the Blend Hand Parameter in the animation tree back down to 0
                await ToSignal(GetTree().CreateTween().BindNode(this).TweenMethod(_SetHandBlend, 1f, 0f, 1.5f), "finished");
            }

            else if (actionName == ModelConsts.Anims.ClownPuppet.WARP_AWAY)
            {
                FireEvent("clatter");
                // Tween the warp material from min value to maximum value
                await ToSignal(GetTree().CreateTween().BindNode(this).TweenMethod(_SetWarpAmount, 0f, 1f, .5f), "finished");
            }

            else if (actionName == ModelConsts.Anims.ClownPuppet.WARP_IN)
            {
                FireEvent("clatter");
                // Tween the warp material from max value to minimum value
                await ToSignal(GetTree().CreateTween().BindNode(this).TweenMethod(_SetWarpAmount, 1f, 0f, .5f), "finished");
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

        private void OnEvent(string eventName)
        {
            if (eventName == "clatter") EmitSignal(nameof(Clattered));
        }
    }
}

