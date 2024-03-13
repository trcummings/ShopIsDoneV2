using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.StatusEffects;

namespace ShopIsDone.PassiveEffects
{
	public partial class EasilyDepressedPassiveEffect : PassiveEffect
	{
        [Export]
        private float _TriggerChance = 0.5f;

        [Export]
        private StatusEffect _DepressedEffect;

        private ActionPointHandler _ApHandler;

        public override void Init(PassiveEffectHandler handler)
        {
            base.Init(handler);
            _ApHandler ??= _Entity.GetComponent<ActionPointHandler>();
        }

        public override void ApplyEffect()
        {
            // Connect to AP handler damage event
            _ApHandler.TookDebtDamage += OnTriggerDepression;
        }

        public override void RemoveEffect()
        {
            // Disconnect from damage event
            _ApHandler.TookDebtDamage -= OnTriggerDepression;
        }

        private void OnTriggerDepression(int _)
        {
            // Probability check
            if (PercentCheck(_TriggerChance))
            {
                // Emit an event sequence to the queue that gives the unit depression
                // for 1 turn
                EmitCommandToQueue(
                    // TODO: Some kind of FX with camera zoom(?)
                    ApplyStatusEffect(_DepressedEffect)
                );
            }

        }
    }
}
