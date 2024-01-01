using System.Linq;
using Godot;
using Godot.Collections;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Positioning;

namespace ShopIsDone.Microgames
{
    public partial class MicrogamePayload : GodotObject
    {
        public Microgame Microgame;
        public IOutcomeHandler[] Targets;
        public IOutcomeHandler Source;
        public Positions Position = Positions.Null;
        public Dictionary<string, Variant> Message = new Dictionary<string, Variant>();
        public bool IsPlayerAggressor = false;
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
            Microgame.Outcomes outcome = Microgame.Outcomes.Loss;

            return new SeriesCommand(
                new AsyncCommand(async () =>
                {
                    if (payload.IsPlayerAggressor) EmitSignal(nameof(AttackStarted));
                    else EmitSignal(nameof(DefenseStarted));

                    _MicrogameManager.RunMicrogame(payload.Microgame, payload.Message);
                    var result = await ToSignal(_MicrogameManager, nameof(_MicrogameManager.MicrogameFinished));
                    outcome = (Microgame.Outcomes)(int)result[0];
                }),
                new DeferredCommand(() => PostMicrogame(payload, outcome))
            );
        }

        private Command PostMicrogame(MicrogamePayload payload, Microgame.Outcomes outcome)
        {
            return new SeriesCommand(
                payload.Source.HandleOutcome(outcome, payload.Targets, payload.Source, payload.Position),
                new DeferredCommand(() => new SeriesCommand(
                    payload.Targets.Select(t => t.HandleOutcome(outcome, payload.Targets, payload.Source, payload.Position)).ToArray())
                )
            );
        }
    }
}