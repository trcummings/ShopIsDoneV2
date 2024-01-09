using System;
using Godot;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.ArenaInteractions;
using ShopIsDone.EntityStates;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Lighting
{
	public partial class LightSwitchInteractionComponent : InteractionComponent
    {
        [Export]
        private LightSwitchComponent _LightSwitch;

        public override Command RunInteraction(
            UnitInteractionHandler handler,
            Dictionary<string, Variant> message = null
        )
        {
            // Get unit state handler
            var stateHandler = handler.Entity.GetComponent<EntityStateHandler>();

            return new SeriesCommand(
                // Change to doing task state
                stateHandler.RunChangeState(StateConsts.Employees.DO_TASK),
                // Flip off the lights
                _LightSwitch.RunFlipLights(),
                // Wait a moment for emphasis
                new WaitForCommand(this, 0.75f),
                // Go back to idle
                stateHandler.RunChangeState(StateConsts.IDLE),
                // Fire off the base finished hook
                base.RunInteraction(handler, message)
            );
        }
    }
}

