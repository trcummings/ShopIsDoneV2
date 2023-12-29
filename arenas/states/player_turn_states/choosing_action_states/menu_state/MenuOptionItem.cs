using Godot;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu
{
	public partial class MenuOptionItem : Control
	{
        // State vars
        public bool IsDisabled = false;
        public MenuAction MenuAction = new MenuAction();

        // Nodes
        private Label _Unselected;
        private Label _Selected;
        private Label _Pressed;
        private Label _DisabledUnselected;
        private Label _DisabledSelected;
        private List<Label> _Labels;
        private SubViewportContainer _CursorContainer;

        public override void _Ready()
        {
            // Ready nodes
            _Unselected = GetNode<Label>("%UnselectedOptionLabel");
            _Selected = GetNode<Label>("%SelectedOptionLabel");
            _Pressed = GetNode<Label>("%PressedOptionLabel");
            _DisabledUnselected = GetNode<Label>("%SelectedDisabledOptionLabel");
            _DisabledSelected = GetNode<Label>("%UnselectedDisabledOptionLabel");
            _Labels = new List<Label>()
            {
                _Unselected,
                _Selected,
                _Pressed,
                _DisabledUnselected,
                _DisabledSelected,
            };
            _CursorContainer = GetNode<SubViewportContainer>("%CursorContainer");
        }

        public void Init(bool isSelected, MenuAction menuAction)
        {
            // Set label text
            foreach (var label in _Labels)
            {
                label.Text = menuAction.Name;
            }

            // Set Menu Action
            MenuAction = menuAction;

            // Set disabled state
            IsDisabled = !menuAction.IsAvailable();

            // Select the proper initial label based on logic
            var labelToShow = _Unselected;
            if (isSelected)
            {
                // Show the cursor
                _CursorContainer.Show();

                // Show the selected label
                labelToShow = _Selected;

                // Unless it's disabled
                if (IsDisabled) labelToShow = _DisabledSelected;
            }
            else
            {
                // Hide the cursor
                _CursorContainer.Hide();

                if (IsDisabled) labelToShow = _DisabledUnselected;
            }

            ShowCurrentLabelState(labelToShow);
        }

        public void SelectOption()
        {
            // Show the cursor
            _CursorContainer.Show();

            if (!IsDisabled)
            {
                ShowCurrentLabelState(_Selected);
            }
            else
            {
                ShowCurrentLabelState(_DisabledSelected);
            }
        }

        public void DeselectOption()
        {
            // Hide the cursor
            _CursorContainer.Hide();

            if (!IsDisabled)
            {
                ShowCurrentLabelState(_Unselected);
            }
            else
            {
                ShowCurrentLabelState(_DisabledUnselected);
            }
        }

        public void PressOption()
        {
            if (!IsDisabled)
            {
                ShowCurrentLabelState(_Pressed);
            }
            else
            {
                ShowCurrentLabelState(_DisabledUnselected);
            }
        }

        private void ShowCurrentLabelState(Label labelToShow)
        {
            foreach (var label in _Labels)
            {
                label.Hide();
            }
            labelToShow.Show();
        }
    }
}
