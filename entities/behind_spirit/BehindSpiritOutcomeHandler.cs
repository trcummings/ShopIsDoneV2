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
    public partial class BehindSpiritOutcomeHandler : NodeComponent, IOutcomeHandler
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

        // Empty outcome
        public Command HandleOutcome(MicrogamePayload payload)
        {
            return new ConditionalCommand(
                // If the player lost
                () => payload.Outcome == Microgame.Outcomes.Loss,
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

        public DamagePayload GetDamage()
        {
            return new DamagePayload()
            {
                Health = 0,
                Defense = 0,
                DrainDefense = 0,
                Damage = 3,
                Drain = 0,
                Piercing = 0
            };
        }
    }
}

