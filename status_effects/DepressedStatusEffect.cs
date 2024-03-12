using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Core.Stats;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.StatusEffects
{
	public partial class DepressedStatusEffect : StatusEffect
	{
        private ActionPointHandler _ApHandler;
        private Modifier _RecoveryMod;

        public override void Init(StatusEffectHandler handler)
        {
            base.Init(handler);
            _ApHandler = _Entity.GetComponent<ActionPointHandler>();
            _RecoveryMod = new Modifier(-_ApHandler.MaxActionPoints, Modifier.ModifierType.Additive);
        }

        public override void ApplyEffect()
        {
            // Add an AP recovery modifier that reduces recovery to 0, drain
            // remaining AP
            _ApHandler.ApRecovery.AddModifier(_RecoveryMod);
            EmitCommandToQueue(new ActionCommand(() => _ApHandler.SpendAPOnAction(_ApHandler.ActionPoints)));
        }

        // Remove the AP recovery modifier that reduces recovery to 0
        public override void RemoveEffect()
        {
            _ApHandler.ApRecovery.RemoveModifier(_RecoveryMod);
        }
    }
}
