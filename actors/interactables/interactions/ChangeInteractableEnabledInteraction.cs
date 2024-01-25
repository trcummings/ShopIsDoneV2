using Godot;
using System;
using System.Linq;

namespace ShopIsDone.Interactables.Interactions
{
    // This is used to disable an interaction at some point, like after a
    // branching path
	[Tool]
	public partial class ChangeInteractableEnabledInteraction : Interaction
	{
        [Export]
        private bool _ToEnabledState;

		[Export]
		private NodePath _InteractablePath;
        private Interactable _Interactable;

        public override void _Ready()
        {
            base._Ready();
            if (Engine.IsEditorHint()) return;
            _Interactable = GetNode<Interactable>(_InteractablePath);
        }

        public override void Execute()
        {
            if (_ToEnabledState) _Interactable.Enable();
            else _Interactable.Disable();

            CallDeferred(nameof(Finish));
        }

        public override string[] _GetConfigurationWarnings()
        {
            if (string.IsNullOrEmpty(_InteractablePath))
            {
                return new string[] { "No Interactable given!" };
            }
            return base._GetConfigurationWarnings();
        }
    }
}
