using System;
using System.Linq;
using Godot;

namespace ShopIsDone.Interactables.Interactions
{
    // This interaction decorates an only child interaction with some behavior
    [Tool]
    public partial class DecoratorInteraction : Interaction
	{
        protected Interaction _DecoratedInteraction;

        public override void _Ready()
        {
            base._Ready();
            // Get the first child of the interaction, and set it to the
            // decorated interaction
            _DecoratedInteraction = GetChildren()
                .OfType<Interaction>()
                .ToList()
                .FirstOrDefault();
        }

        public override string[] _GetConfigurationWarnings()
        {
            if (_DecoratedInteraction == null)
            {
                return new string[] { "Needs one Interaction as child to decorate!" };
            }
            else if (GetChildren().OfType<Interaction>().Count() > 1)
            {
                return new string[] { "More than one Interaction as child of DecoratorInteraction. Only first child will be decorated." };
            }
            return base._GetConfigurationWarnings();
        }
    }
}

