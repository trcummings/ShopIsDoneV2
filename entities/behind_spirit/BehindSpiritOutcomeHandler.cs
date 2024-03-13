using System;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Microgames;
using ShopIsDone.Core;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;
using Godot;
using System.Threading.Tasks;

namespace ShopIsDone.Entities.BehindSpirit
{
    public partial class BehindSpiritOutcomeHandler : OutcomeHandler
    {
        [Inject]
        private CameraService _CameraService;

        [Export]
        private Camera3D _Camera;

        [Export]
        private AnimationPlayer _AnimPlayer;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        public override Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload)
        {
            return target.ReceiveDamage(GetDamage(outcomePayload));
        }

        // Before we resovle the damage, play the neck cracking animation
        public override Command BeforeOutcomeResolution(MicrogamePayload payload)
        {
            return new ConditionalCommand(
                // If we won the microgame
                payload.WonMicrogame,
                // Run animation
                new SeriesCommand(
                    // Swap camera angles
                    new ActionCommand(_Camera.MakeCurrent),
                    // Run animation and wait for it to finish
                    new AsyncCommand(RunAnimation),
                    // Set camera back to main camera
                    new ActionCommand(_CameraService.MakeCurrent),
                    // Reset
                    new ActionCommand(() => _AnimPlayer.Play("RESET"))
                )
            );
        }

        private async Task RunAnimation()
        {
            _AnimPlayer.Play("grab");
            await ToSignal(_AnimPlayer, "animation_finished");
        }

        public override DamagePayload GetDamage(MicrogamePayload outcomePayload)
        {
            return new DamagePayload()
            {
                Damage = outcomePayload.WonMicrogame() ? 3 : 0,
                Source = Entity
            };
        }
    }
}

