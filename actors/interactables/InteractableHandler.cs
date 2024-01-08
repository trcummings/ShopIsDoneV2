using Godot;
using System;

namespace ShopIsDone.Interactables
{
    // Used by an actor in a free move state to interact with contextual
    // interactions
    public partial class InteractableHandler : Node
    {
        [Signal]
        public delegate void InteractionBeganEventHandler();

        [Signal]
        public delegate void InteractionFinishedEventHandler();

        [Export]
        private Area3D _InteractableDetector;

        private Interactable _CurrentInteractable;
        private Callable _BeginInteraction;
        private Callable _FinishInteraction;

        public override void _Ready()
        {
            base._Ready();

            // Create callables
            _BeginInteraction = Callable.From(() => EmitSignal(nameof(InteractionBegan)));
            _FinishInteraction = Callable.From(() => EmitSignal(nameof(InteractionFinished)));

            // Initially deactivate
            Deactivate();

            // Connect to detector events
            _InteractableDetector.AreaEntered += OnInteractableEntered;
            _InteractableDetector.AreaExited += OnInteractableExited;
        }

        public void Interact()
        {
            if (_CurrentInteractable == null) return;

            // Connect to interactable events
            _CurrentInteractable.Connect(
                nameof(_CurrentInteractable.InteractionBegan),
                _BeginInteraction,
                (uint)ConnectFlags.OneShot
            );
            _CurrentInteractable.Connect(
                nameof(_CurrentInteractable.InteractionFinished),
                _FinishInteraction,
                (uint)ConnectFlags.OneShot
            );

            _CurrentInteractable.StartInteraction();
        }

        public void Activate()
        {
            _InteractableDetector.SetDeferred("monitoring", true);
        }

        public void Deactivate()
        {
            _InteractableDetector.SetDeferred("monitoring", false);
        }

        private void OnInteractableEntered(Area3D body)
        {
            if (body is Interactable interactable)
            {
                // Set current interactable
                _CurrentInteractable = interactable;
                // Hover interactable
                interactable.Hover();
            }
        }

        private void OnInteractableExited(Area3D body)
        {
            if (body is Interactable interactable)
            {
                // If our current interactable exited, null it out
                if (_CurrentInteractable == interactable) _CurrentInteractable = null;
                // Unhover the interactable
                interactable.Unhover();
            }
        }
    }
}
