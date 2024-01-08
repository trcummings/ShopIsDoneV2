using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Interactables
{
    // This class describes a type of contextual interaction within the level
    // outside of battle
    public partial class Interactable : Area3D
    {
        [Signal]
        public delegate void InteractableHoveredEventHandler();

        [Signal]
        public delegate void InteractableUnhoveredEventHandler();

        [Signal]
        public delegate void InteractionBeganEventHandler();

        [Signal]
        public delegate void InteractionFinishedEventHandler();

        [Export]
        public string Prompt = "Interact";

        // State
        private bool _IsHovered = false;

        public virtual void Init()
        {
            InjectionProvider.Inject(this);
        }

        public void Hover()
        {
            _IsHovered = true;
            EmitSignal(nameof(InteractableHovered));
        }

        public void Unhover()
        {
            _IsHovered = false;
            EmitSignal(nameof(InteractableUnhovered));
        }

        public virtual void StartInteraction()
        {
            EmitSignal(nameof(InteractionBegan));
        }

        public virtual void FinishInteraction()
        {
            EmitSignal(nameof(InteractionFinished));
        }

        public void Disable()
        {
            SetDeferred("monitorable", false);
            SetDeferred("monitoring", false);
            // If we're hovered, unhover
            if (_IsHovered) Unhover();
        }

        public void Enable()
        {
            SetDeferred("monitorable", true);
            SetDeferred("monitoring", true);
        }
    }
}