using Godot;
using ShopIsDone.Core.Stats;
using ShopIsDone.Microgames.Outcomes;

namespace ShopIsDone.PassiveEffects
{
    // Flat reduction to unit chance to successfully deal damage to a target on
    // completion of a microgame
    public partial class IncompetentPassiveEffect : PassiveEffect
    {
        [Export]
        public float IncompetenceAmount = 0.5f;

        private EmployeeOutcomeHandler _OutcomeHandler;
        private Modifier _IncompetenceMod;

        public override void Init(PassiveEffectHandler handler)
        {
            base.Init(handler);
            _OutcomeHandler = _Entity.GetComponent<EmployeeOutcomeHandler>();
            _IncompetenceMod = new Modifier(-IncompetenceAmount, Modifier.ModifierType.Additive);
        }

        public override void ApplyEffect()
        {
            // Add the incompetence modifier to the outcome handler's
            // competence stat
            _OutcomeHandler.Competence.AddModifier(_IncompetenceMod);
        }

        public override void RemoveEffect()
        {
            // Remove the incompetence modifier
            _OutcomeHandler.Competence.RemoveModifier(_IncompetenceMod);
        }
    }
}
