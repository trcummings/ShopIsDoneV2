using Godot;
using ShopIsDone.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Arenas.UI
{
    public partial class PlayerPawnUIContainer : Control
    {
        [Export]
        public PackedScene PawnUIElementScene;

        // Nodes
        private Control _PawnUIElements;
        private Control _SelectUnitUI;

        public override void _Ready()
        {
            // Ready nodes
            _PawnUIElements = GetNode<Control>("%PawnUIElements");
            _SelectUnitUI = GetNode<Control>("%SelectUnitUI");
        }

        public void Init(List<LevelEntity> pawns)
        {
            // Clear away any previous pawn UI elements
            foreach (var ui in _PawnUIElements.GetChildren().OfType<PlayerPawnUI>())
            {
                ui.Hide();
                _PawnUIElements.RemoveChild(ui);
                ui.QueueFree();
            }

            // Init PlayerPawnUI
            foreach (var pawn in pawns)
            {
                var ui = PawnUIElementScene.Instantiate<PlayerPawnUI>();
                _PawnUIElements.AddChild(ui);
                ui.Init(pawn);
            }

        }

        public void SelectPawnElement(LevelEntity pawn)
        {
            // Select this one and deselect all the others
            foreach (var ui in _PawnUIElements.GetChildren().OfType<PlayerPawnUI>())
            {
                ui.SelectPawnUI(ui.Pawn == pawn);
            }

            // Modulate if we show "Select Unit"
            if (pawn != null) _SelectUnitUI.Show();
            else _SelectUnitUI.Hide();
        }
    }
}