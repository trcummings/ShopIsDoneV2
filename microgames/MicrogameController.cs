using System.Linq;
using Godot;
using Godot.Collections;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Positioning;

namespace ShopIsDone.Microgames
{
    public partial class MicrogamePayload : Resource
    {
        public Microgame Microgame;
        public Microgame.Outcomes Outcome;
        public IOutcomeHandler[] Targets;
        public IOutcomeHandler Source;
        public Positions Position = Positions.Null;
        public Dictionary<string, Variant> Message = new Dictionary<string, Variant>();
        public bool IsPlayerAggressor = false;

        public bool WonMicrogame()
        {
            return Outcome == Microgame.Outcomes.Win;
        }

        public bool LostMicrogame()
        {
            return Outcome == Microgame.Outcomes.Loss;
        }
    }

    public partial class MicrogameController : Node, IService
    {
        [Signal]
        public delegate void AttackStartedEventHandler();

        [Signal]
        public delegate void DefenseStartedEventHandler();

        [Export]
        private MicrogameManager _MicrogameManager;

        public Command RunMicrogame(MicrogamePayload payload)
        {
            // Initially set outcome to loss
            Microgame.Outcomes outcome = Microgame.Outcomes.Loss;

            return new SeriesCommand(
                new AsyncCommand(async () =>
                {
                    // Init battle signals
                    if (payload.IsPlayerAggressor) EmitSignal(nameof(AttackStarted));
                    else EmitSignal(nameof(DefenseStarted));

                    // Run microgame
                    _MicrogameManager.RunMicrogame(payload.Microgame, payload.Message);

                    // Pull outcome from result
                    var result = await ToSignal(_MicrogameManager, nameof(_MicrogameManager.MicrogameFinished));
                    outcome = (Microgame.Outcomes)(int)result[0];
                    // Set outcome in payload
                    payload.Outcome = outcome;
                }),
                new DeferredCommand(() => ResolveDamage(payload))
            );
        }

        private Command ResolveDamage(MicrogamePayload payload)
        {
            return new DeferredCommand(() => {
                // Pull the source and targets out of the payload
                var targets = payload.Targets;
                var source = payload.Source;

                // Create an inverted outcome payload for the source, as the
                // source is always the "producer" of the microgame and therefore
                // a source win is a player loss, and vice versa.
                var sourcePayload = (MicrogamePayload)payload.Duplicate();
                sourcePayload.Outcome = payload.WonMicrogame()
                    ? Microgame.Outcomes.Loss
                    : Microgame.Outcomes.Win;

                return new SeriesCommand(
                    // First, run the before outcome hook for all handlers
                    new SeriesCommand(
                        source.BeforeOutcomeResolution(sourcePayload),
                        new SeriesCommand(
                            targets.Select(h => h.BeforeOutcomeResolution(payload)).ToArray()
                        )
                    ),
                    // Next, resolve damage from the targets to the source
                    new SeriesCommand(
                        targets
                            .Select(t => t.InflictDamage(source.DamageTarget, payload))
                            .ToArray()
                    ),
                    // Then, resolve damage from the source to the targets
                    new SeriesCommand(
                        targets
                            .Select(t => source.InflictDamage(t.DamageTarget, sourcePayload))
                            .ToArray()
                    ),
                    // Finally, run the after outcome hook for all handlers
                    new SeriesCommand(
                        source.AfterOutcomeResolution(sourcePayload),
                        new SeriesCommand(
                            targets.Select(h => h.AfterOutcomeResolution(payload)).ToArray()
                        )
                    )
                );
            });
        }
    }
}