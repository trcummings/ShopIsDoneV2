using Godot;
using System;
using System.Linq;

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

        [Export]
        private RayCast3D _WallRaycast;

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
            _WallRaycast.Enabled = true;
            _InteractableDetector.SetDeferred("monitoring", true);
            SetPhysicsProcess(true);
            _CurrentInteractable?.Hover();
        }

        public void Deactivate()
        {
            _WallRaycast.Enabled = false;
            _InteractableDetector.SetDeferred("monitoring", false);
            SetPhysicsProcess(false);
            _CurrentInteractable?.Unhover();
        }

        public override void _PhysicsProcess(double _)
        {
            // Null out interactable if colliding
            if (_WallRaycast.IsColliding() && _CurrentInteractable != null)
            {
                _CurrentInteractable.Unhover();
                _CurrentInteractable = null;
            }

            // Get the overlapping interactable
            var interactable = _InteractableDetector
                .GetOverlappingAreas()
                .OfType<Interactable>()
                .FirstOrDefault();

            // If we have an interactable, set the interactable
            if (interactable != null)
            {
                // Could be null, so null check the function call
                _CurrentInteractable?.Unhover();
                // Set and hover
                _CurrentInteractable = interactable;
                _CurrentInteractable.Hover();
            }
        }
    }
}
