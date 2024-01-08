using Godot;
using ShopIsDone.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace ShopIsDone.Arenas.UI
{
	public partial class EndTurnUnitInfo : Control
	{
		[Export]
		private Label _Title;

		[Export]
		private Label _ActionTemplate;

		[Export]
		private Control _ActionContainer;

		public void SetInfo(string title, List<string> actions)
		{
			_Title.Text = title;

            _ActionContainer.RemoveChildrenOfType<Control>();
            foreach (var action in actions)
            {
                var scene = (Label)_ActionTemplate.Duplicate();
				scene.Text = action;
				scene.Show();
                _ActionContainer.AddChild(scene);
            }
        }
	}
}
