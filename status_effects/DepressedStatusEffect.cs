using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Core.Stats;
using ShopIsDone.Utils.Commands;
using System;

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

        // Add an AP recovery modifier that reduces recovery to 0
        public override Command ApplyEffect()
        {
            return new ActionCommand(() =>
            {
                GD.Print($"{_Entity.EntityName} is Depressed! No AP refill next turn.");
                _ApHandler.ApRecovery.AddModifier(_RecoveryMod);
            });
        }

        // Remove the AP recovery modifier that reduces recovery to 0
        public override Command RemoveEffect()
        {
            return new ActionCommand(() =>
            {
                GD.Print($"{_Entity.EntityName} is no longer depressed.");
                _ApHandler.ApRecovery.RemoveModifier(_RecoveryMod);
            });
        }
    }
}
