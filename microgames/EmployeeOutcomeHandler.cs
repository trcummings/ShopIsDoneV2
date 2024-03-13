using System;
using ShopIsDone.ActionPoints;
using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ApConsts = ShopIsDone.ActionPoints.Consts;
using ShopIsDone.Core.Stats;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Levels;
using ShopIsDone.Widgets;

namespace ShopIsDone.Microgames.Outcomes
{
    public class ActionPointDamageTarget : IDamageTarget
    {
        public LevelEntity Entity;
        public ActionPointHandler ApHandler;

        public Command ReceiveDamage(DamagePayload payload)
        {
            return new DeferredCommand(() =>
            {
                // Concat payload message to damage payload
                var message = new Dictionary<string, Variant>()
                {
                    { ApConsts.DAMAGE_SOURCE, payload.Source },
                    { ApConsts.DEBT_DAMAGE, payload.Damage }
                };
                if (payload.Message != null)
                {
                    foreach (var kv in payload.Message) message[kv.Key] = kv.Value;
                }

                return new ConditionalCommand(
                    // Are we still active check
                    Entity.IsActive,
                    // Deal damage to ourselves
                    ApHandler.TakeAPDamage(message)
                );
            });
        }
    }

    public partial class EmployeeOutcomeHandler : OutcomeHandler
    {
        [Signal]
        public delegate void FailedDamageEventHandler();

        [Export]
        private ActionPointHandler _ActionPointHandler;

        // Stats
        [Export]
        public int Efficacy = 1;

        [Export]
        public int Cordiality = 1;

        public Stat Competence = new Stat(1f);

        [Inject]
        private LevelRngService _RngService;


        [Inject]
        private EntityWidgetService _WidgetService;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        protected override IDamageTarget GetDamageTarget()
        {
            return new ActionPointDamageTarget()
            {
                Entity = Entity,
                ApHandler = _ActionPointHandler
            };
        }

        public override Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload)
        {
            return new DeferredCommand(() =>
            {
                var competence = Competence.GetValue();
                var wasIncompetent =
                    // If we won
                    outcomePayload.WonMicrogame() &&
                    // And our competence is a liability
                    competence < 1f &&
                    // And we fail an RNG check
                    _RngService.PercentCheck(competence);

                return new IfElseCommand(
                    () => wasIncompetent,
                    // Then show the "Failed" text / FX
                    new SeriesCommand(
                        new ActionCommand(() => EmitSignal(nameof(FailedDamage))),
                        new AsyncCommand(() =>
                            _WidgetService.PopupLabelAsync(
                                Entity.WidgetPoint,
                                "Failed!"
                            )
                        )
                    ),
                    // Otherwise, inflict damage as normal
                    target.ReceiveDamage(GetDamage(outcomePayload))
                );
            });
        }

        public override DamagePayload GetDamage(MicrogamePayload outcomePayload)
        {
            // Copy over the message to the damage payload's message
            var message = new Dictionary<string, Variant>();
            foreach (var kv in outcomePayload?.Message ?? new Dictionary<string, Variant>())
            {
                message[kv.Key] = kv.Value;
            }

            return new DamagePayload()
            {
                // If we won, pass along full damage
                Damage = outcomePayload.WonMicrogame() ? 1 : 0,
                Source = Entity,
                Message = message
            };
        }
    }
}

