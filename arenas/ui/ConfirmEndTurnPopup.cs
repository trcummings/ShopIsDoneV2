using Godot;
using ShopIsDone.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Arenas.UI
{
    public partial class ConfirmEndTurnPopup : Control
    {
        [Export]
        private PackedScene _UnitInfoScene;

        [Export]
        private Control _UnitInfoContainer;

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
    }
}

