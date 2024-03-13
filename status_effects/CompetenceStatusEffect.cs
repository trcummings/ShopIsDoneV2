using Godot;
using ShopIsDone.Core.Stats;
using ShopIsDone.Microgames.Outcomes;

namespace ShopIsDone.StatusEffects
{
    // Adjusts the employee's competence stat
	public partial class CompetenceStatusEffect : StatusEffect
	{
        [Export]
        public float Amount;

        private Modifier _CompetenceMod;
        private EmployeeOutcomeHandler _OutcomeHandler;

        public override void Init(StatusEffectHandler handler)
        {
            base.Init(handler);
            _OutcomeHandler = _Entity.GetComponent<EmployeeOutcomeHandler>();
            _CompetenceMod = new Modifier(Amount, Modifier.ModifierType.Additive);
        }

        public override void ApplyEffect()
        {
            // Add the modifier to the outcome handler's competence stat
            _OutcomeHandler.Competence.AddModifier(_CompetenceMod);
        }

        public override void RemoveEffect()
        {
            // Remove the modifier
            _OutcomeHandler.Competence.RemoveModifier(_CompetenceMod);
        }
    }
}