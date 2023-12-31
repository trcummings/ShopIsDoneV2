using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu
{
    public partial class OptionsMenu : Control
    {
        public class Props
        {
            public int InitialSelectedIndex = 0;
            public string MenuTitle;
            public List<ArenaAction> Actions;
        }

        [Signal]
        public delegate void ConfirmedSelectionEventHandler(ArenaAction menuAction);

        [Signal]
        public delegate void CanceledSelectionEventHandler();

        [Signal]
        public delegate void SelectedDisabledOptionEventHandler();

        [Signal]
        public delegate void ChangedSelectionEventHandler(ArenaAction menuAction);

        [Export]
        public PackedScene MenuOptionScene;

        // State
        private int _Idx = 0;

        // Nodes
        public IEnumerable<MenuOptionItem> Options
        { get { return _Options.GetChildren().OfType<MenuOptionItem>(); } }
        private Control _Options;
        private Label _MenuTitle;

        public override void _Ready()
        {
            // Stop processing
            SetProcess(false);

            // Ready nodes
            _Options = GetNode<Control>("%Options");
            _MenuTitle = GetNode<Label>("%MenuTitle");
        }

        public override void _Process(double delta)
        {
            // Accept selected option
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // Get currently selected option
                var selectedOption = _Options.GetChild<MenuOptionItem>(_Idx);

                // If option is disabled, emit disabled signal
                if (selectedOption.IsDisabled)
                {
                    EmitSignal(nameof(SelectedDisabledOption));
                }
                // Otherwise, press the option and emit signal
                else
                {
                    selectedOption.PressOption();
                    EmitSignal(nameof(ConfirmedSelection), selectedOption.Action);
                }

                return;
            }

            // Cancel action selection
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // Emit canceled signal
                EmitSignal(nameof(CanceledSelection));
                return;
            }


            // Move up
            if (Input.IsActionJustPressed("ui_down"))
            {
                // Get list size
                var size = _Options.GetChildren().OfType<MenuOptionItem>().ToList().Count;

                // Select next option
                var nextOptionIdx = GetNextOption(_Idx, size);
                SelectOption(nextOptionIdx);
                return;
            }

            // Move down
            if (Input.IsActionJustPressed("ui_up"))
            {
                // Get list size
                var size = _Options.GetChildren().OfType<MenuOptionItem>().ToList().Count;

                // Select previous option
                var nextOptionIdx = GetPreviousOption(_Idx, size);
                SelectOption(nextOptionIdx);
                return;
            }
        }

        public void Activate(Props props)
        {
            // Set initial index
            _Idx = props.InitialSelectedIndex;

            // Set Title
            _MenuTitle.Text = props.MenuTitle;

            // Create Options
            _Options.RemoveChildrenOfType<MenuOptionItem>();
            for (int i = 0; i < props.Actions.ToList().Count; i++)
            {
                var menuAction = props.Actions[i];

                // Create the option scene
                var newOptionScene = MenuOptionScene.Instantiate<MenuOptionItem>();

                // Add it to the options node
                _Options.AddChild(newOptionScene);

                // Intialize the option
                newOptionScene.Init(i == _Idx, menuAction);
            }

            // Select the option
            SelectOption(_Idx);

            // Start processing
            SetProcess(true);
        }

        public void Deactivate()
        {
            // Destroy all options
            _Options.RemoveChildrenOfType<MenuOptionItem>();

            // Stop processing
            SetProcess(false);
        }

        private void SelectOption(int idx)
        {
            // Deselect all other options
            foreach (var option in Options) option.DeselectOption();

            // Select the current option and update the index
            var nextOption = _Options.GetChild<MenuOptionItem>(idx);
            nextOption.SelectOption();
            _Idx = idx;

            // Emit signal that this one was selected
            EmitSignal(nameof(ChangedSelection), nextOption.Action);
        }

        private int GetNextOption(int idx, int size)
        {
            var nextIdx = 0;
            if (idx + 1 < size) nextIdx = idx + 1;
            return nextIdx;
        }

        private int GetPreviousOption(int idx, int size)
        {
            var nextIdx = idx - 1;
            if (nextIdx < 0) nextIdx = size - 1;
            return nextIdx;
        }
    }
}