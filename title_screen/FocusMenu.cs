using System;
using Godot;

namespace ShopIsDone.TitleScreen
{
    public partial class FocusMenu : CanvasLayer
    {
        [Signal]
        public delegate void ChangedSelectionEventHandler();

        [Signal]
        public delegate void InvalidConfirmEventHandler();

        [Signal]
        public delegate void ConfirmedEventHandler();

        [Signal]
        public delegate void CanceledEventHandler();

        // State
        protected Control _FocusContainer;

        protected void SetFocusContainer(Control focusContainer)
        {
            // Set focus container
            _FocusContainer = focusContainer;

            // Focus first element
            var firstElement = _FocusContainer.FindNextValidFocus();
            if (firstElement != null) firstElement.CallDeferred("grab_focus");
        }

        public override void _Process(double delta)
        {
            // Return early if we don't have a focused element
            var focusedElement = GetViewport().GuiGetFocusOwner();
            if (focusedElement == null) return;

            // Accumulate input vars
            var downPressed = Input.IsActionJustPressed("ui_down");
            var upPressed = Input.IsActionJustPressed("ui_up");
            var leftPressed = Input.IsActionJustPressed("ui_left");
            var rightPressed = Input.IsActionJustPressed("ui_right");
            var acceptPressed = Input.IsActionJustPressed("ui_accept");
            var cancelPressed = Input.IsActionJustPressed("ui_cancel");

            // Handle UI input based on the type of selection
            if (focusedElement is Button button)
            {
                if (downPressed) EmitSignal(nameof(ChangedSelection));
                else if (upPressed) EmitSignal(nameof(ChangedSelection));
                else if (acceptPressed)
                {
                    if (button.Disabled) EmitSignal(nameof(InvalidConfirm));
                    else EmitSignal(nameof(Confirmed));
                }
                else if (cancelPressed)
                {
                    EmitSignal(nameof(Canceled));
                    OnBackRequested();
                }
            }

            else if (focusedElement is Slider slider)
            {
                if (downPressed) EmitSignal(nameof(ChangedSelection));
                else if (upPressed) EmitSignal(nameof(ChangedSelection));
                else if (leftPressed) EmitSignal(nameof(ChangedSelection));
                else if (rightPressed) EmitSignal(nameof(ChangedSelection));
                else if (cancelPressed)
                {
                    EmitSignal(nameof(Canceled));
                    OnBackRequested();
                }
            }

            else if (focusedElement is OptionButton optionButton)
            {
                if (downPressed) EmitSignal(nameof(ChangedSelection));
                else if (upPressed) EmitSignal(nameof(ChangedSelection));
                else if (acceptPressed)
                {
                    if (optionButton.Disabled) EmitSignal(nameof(InvalidConfirm));
                    else EmitSignal(nameof(Confirmed));
                }
                else if (cancelPressed)
                {
                    EmitSignal(nameof(Canceled));
                    // Only request back if the menu is closed
                    if (!optionButton.ButtonPressed) OnBackRequested();
                }
            }
        }

        protected virtual void OnBackRequested()
        {
            // On press "cancel" behavior here
        }
    }


}
