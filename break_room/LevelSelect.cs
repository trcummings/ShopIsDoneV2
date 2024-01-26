using Godot;
using System;
using Godot.Collections;
using ShopIsDone.UI;
using ShopIsDone.Core.Data;

namespace ShopIsDone.BreakRoom
{
    public partial class LevelSelect : FocusMenu
    {
        [Signal]
        public delegate void LevelChangeRequestedEventHandler(PackedScene level);

        private Button _LevelButtonTemplate;
        private Control _LevelButtons;
        private LevelDb _LevelDb;

        public override void _Ready()
        {
            base._Ready();
            _LevelButtonTemplate = GetNode<Button>("%LevelButtonTemplate");
            _LevelButtons = GetNode<Control>("%LevelButtons");
            _LevelDb = LevelDb.GetLevelDb(this);

            // Create buttons and connect to them
            foreach (var levelItem in _LevelDb.GetLevels())
            {
                var button = (Button)_LevelButtonTemplate.Duplicate();
                button.Text = levelItem.Label;
                button.Show();
                button.Pressed += () =>
                {
                    EmitSignal(nameof(LevelChangeRequested), levelItem.Id);
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

