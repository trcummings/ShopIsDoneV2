using Godot;
using ShopIsDone.ActionPoints;
using ApConsts = ShopIsDone.ActionPoints.Consts;
using Godot.Collections;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.PassiveEffects
{
    // Chance to deal damage to bearer when they spend AP down to 0 in a single
    // turn
    public partial class ChronicPainPassiveEffect : PassiveEffect
    {
        [Export]
        private float _TriggerChance = 0.25f;

        private ActionPointHandler _ApHandler;

        // State
        private int _ApUponRefill = 0;
        private bool _TriggeredThisTurn = false;

        public override void Init(PassiveEffectHandler handler)
        {
            base.Init(handler);
            _ApHandler ??= _Entity.GetComponent<ActionPointHandler>();
        }

        public override void ApplyEffect()
        {
            // Connect to AP events
            _ApHandler.SpentAp += OnSpendAp;
            _ApHandler.RefilledAp += OnRefillAp;
        }

        public override void RemoveEffect()
        {
            // Disconnect from AP events
            _ApHandler.SpentAp -= OnSpendAp;
            _ApHandler.RefilledAp -= OnRefillAp;
        }

        private void OnSpendAp(int _amount)
        {
            // Ignore if AP was 0 upon refill, because we had none to spend, or
            // if we were already triggered this turn
            if (_ApUponRefill == 0 || _TriggeredThisTurn) return;

            // If current AP after spend is 0 and see if it will trigger
            if (_ApHandler.ActionPoints == 0)
            {
                // Set trigerred flag
                // NB: We only want to test it once per turn, not every single
                // time there's AP spend
                _TriggeredThisTurn = true;

                // Roll chance to inflict chronic pain damage
                if (PercentCheck(_TriggerChance))
                {
                    EmitCommandToQueue(
                        new SeriesCommand(
                            // TODO: Camera zoom, popup effect of some kind

                            // Take damage
                            _ApHandler.TakeAPDamage(new Dictionary<string, Variant>()
                            {
                                { ApConsts.DAMAGE_SOURCE, _Entity },
                                { ApConsts.DEBT_DAMAGE, 1 }
                            }),
                            new WaitForCommand(_Entity, 0.5f)
                        )
                    );
                }
            }
        }

        private void OnRefillAp(int _prevPoints, int _refillAmount)
        {
            // Persist the amount of AP we had upon refill
            _ApUponRefill = _ApHandler.ActionPoints;
            // Reset if it was triggered this turn
            _TriggeredThisTurn = false;
        }
    }
}
