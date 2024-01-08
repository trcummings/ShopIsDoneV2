using Godot;
using ShopIsDone.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Arenas.UI
{
    public partial class ConfirmEndTurnPopup : Control
    {
        [Signal]
        public delegate void AcceptedEventHandler();

        [Signal]
        public delegate void CanceledEventHandler();

        [Export]
        private PackedScene _UnitInfoScene;

        [Export]
        private Control _UnitInfoContainer;

        public override void _Ready()
        {
            base._Ready();
            SetProcess(false);
        }

        public void Init(List<(string, List<string>)> unitInfo)
        {
            // Clear container
            _UnitInfoContainer.RemoveChildrenOfType<Control>();
            foreach (var (unit, actions) in unitInfo)
            {
                var scene = _UnitInfoScene.Instantiate<EndTurnUnitInfo>();
                scene.SetInfo(unit, actions);
                _UnitInfoContainer.AddChild(scene);
            }
        }

        public override void _Process(double delta)
        {
            // Wait for confirmation / decline input
            if (Input.IsActionJustPressed("ui_accept"))
            {
                Deactivate();
                EmitSignal(nameof(Accepted));
            }

            if (Input.IsActionJustPressed("ui_cancel"))
            {
                Deactivate();
                EmitSignal(nameof(Canceled));
            }
        }

        public void Activate()
        {
            SetProcess(true);
            Show();
        }

        public void Deactivate()
        {
            SetProcess(false);
            Hide();
        }
    }
}

