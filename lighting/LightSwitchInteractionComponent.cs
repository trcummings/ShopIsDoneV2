using System;
using Godot;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.ArenaInteractions;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.Models;

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
            // Get interacting unit's model
            var model = handler.GetComponent<ModelComponent>();

            return new SeriesCommand(
                // Change to doing task state
                model.RunPerformAction(StateConsts.Employees.DO_TASK),
                // Flip off the lights
                _LightSwitch.RunFlipLights(),
                // Wait a moment for emphasis
                new WaitForCommand(this, 0.75f),
                // Fire off the base finished hook
                base.RunInteraction(handler, message)
            );
        }
    }
}

