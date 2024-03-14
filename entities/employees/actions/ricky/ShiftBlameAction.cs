using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ActionConsts = ShopIsDone.Actions.Consts;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.Models;
using ShopIsDone.StatusEffects;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Entities.Employees.Actions
{
    // This applies a competency status effect to the user, and incompetency to
    // the target
	public partial class ShiftBlameAction : ArenaAction
    {
        [Export]
        public CompetenceStatusEffect CompetenceEffect;

        [Export]
        public CompetenceStatusEffect IncompetenceEffect;

        [Inject]
        private CameraService _Camera;

        private StatusEffectHandler _StatusHandler;
        private EntityStateHandler _StateHandler;
        private ModelComponent _ModelComponent;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _StatusHandler = Entity.GetComponent<StatusEffectHandler>();
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
            _ModelComponent = Entity.GetComponent<ModelComponent>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<EntityStateHandler>() &&
                entity.HasComponent<StatusEffectHandler>() &&
                entity.HasComponent<ModelComponent>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<StatusEffectHandler>();
        }

        // Visible in menu if we're in the idle state, as most actions are
        public override bool IsVisibleInMenu()
        {
            // TODO: Make this unavailable if the unit has Puppetification status
            // effect
            return _StateHandler.IsInState(StateConsts.IDLE);
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get the target from the message
            var target = (LevelEntity)message[ActionConsts.TARGET];
            var targetStatusHandler = target.GetComponent<StatusEffectHandler>();

            // Create competence status effect
            var competence = (CompetenceStatusEffect)CompetenceEffect.Duplicate();
            // Adjust properties to increase competence but only last this turn
            competence.Amount = 0.5f;
            competence.Duration = 0;

            // Create incompetence status effect
            var incompetence = (CompetenceStatusEffect)IncompetenceEffect.Duplicate();
            // Adjust properties to decrease competence but to last next turn
            // too
            incompetence.Amount = -0.5f;
            incompetence.Duration = 1;

            return new SeriesCommand(
                // Mark action as used
                base.Execute(message),
                // Run use ability animation
                _ModelComponent.RunPerformAction(StateConsts.Employees.USE_ABILITY),
                // Move camera to target
                _Camera.PanToTemporaryCameraTarget(
                    target,
                    // Give target incompetence effect
                    targetStatusHandler.ApplyEffect(incompetence)
                ),
                // Give self competence effect
                _StatusHandler.ApplyEffect(competence)
            );
        }
    }
}
