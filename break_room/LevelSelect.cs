using Godot;
using System;
using Godot.Collections;
using ShopIsDone.UI;

namespace ShopIsDone.BreakRoom
{
    public partial class LevelSelect : FocusMenu
    {
        [Signal]
        public delegate void LevelChangeRequestedEventHandler(PackedScene level);

        [Export]
        private Array<LevelSelectItem> _Levels = new Array<LevelSelectItem>();

        private Button _LevelButtonTemplate;
        private Control _LevelButtons;

        public override void _Ready()
        {
            base._Ready();
            _LevelButtonTemplate = GetNode<Button>("%LevelButtonTemplate");
            _LevelButtons = GetNode<Control>("%LevelButtons");

            // Create buttons and connect to them
            foreach (var levelItem in _Levels)
            {
                var button = (Button)_LevelButtonTemplate.Duplicate();
                button.Text = levelItem.Label;
                button.Show();
                button.Pressed += () =>
                {
                    EmitSignal(nameof(LevelChangeRequested), levelItem.LevelScene);
                };
                _LevelButtons.AddChild(button);
            }

            // Deactivate
            Deactivate();
        }

        public override void Activate()
        {
            base.Activate();
            SetFocusContainer(_LevelButtons);
        }
    }
}

