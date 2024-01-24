using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;
using ShopIsDone.Interactables.Interactions;

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
        private bool _IsOneShot = false;

        [Export]
        private Interaction _Interaction;

        [Export]
        private string _Prompt = "Interact";

        private Label3D _PromptLabel;

        // State
        private bool _IsHovered = false;

        public override void _Ready()
        {
            base._Ready();
            _PromptLabel = GetNode<Label3D>("%PromptLabel");
            _PromptLabel.Text = _Prompt;
        }

        public virtual void Init()
        {
            InjectionProvider.Inject(this);
        }

        public void Hover()
        {
            // Ignore if disabled
            if (!Monitorable) return;

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

            // Unhover for the duration of the interaction
            Unhover();

            // Disable if one shot
            if (_IsOneShot) Disable();

            // Run interaction
            if (_Interaction != null)
            {
                _Interaction.Connect(
                    nameof(_Interaction.Finished),
                    Callable.From(FinishInteraction),
                    (uint)ConnectFlags.OneShot
                );
                _Interaction.Execute();
            }
            // If we don't have one just defer a finish call
            else CallDeferred(nameof(FinishInteraction));
        }

        private void FinishInteraction()
        {
            EmitSignal(nameof(InteractionFinished));

            // If it's not a one shot, show the hover again
            if (!_IsOneShot) Hover();
        }

        public void Disable()
        {
            SetDeferred("monitorable", false);
            // If we're hovered, unhover
            if (_IsHovered) Unhover();
        }

        public void Enable()
        {
            SetDeferred("monitorable", true);
        }
    }
}