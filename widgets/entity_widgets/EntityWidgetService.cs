using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Widgets
{
	public partial class EntityWidgetService : Node, IService
    {
		[Export]
		private PackedScene _PopupLabelScene;

        [Export]
        private PackedScene _TalkingEffectScene;

        [Export]
        private PackedScene _AlertEffectScene;

        public void PopupLabel(Node3D point, string labelText)
        {
            // Create scene
            var scene = _PopupLabelScene.Instantiate<PopupLabel>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            // Remove it when done
            scene.Connect(
                nameof(scene.Finished),
                Callable.From(scene.QueueFree),
                (uint)ConnectFlags.OneShot
            );
            scene.PopupText(labelText, Colors.White, Colors.Black);
        }

        public async Task PopupLabelAsync(Node3D point, string labelText)
        {
            // Create scene
            var scene = _PopupLabelScene.Instantiate<PopupLabel>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            await scene.PopupTextAsync(labelText, Colors.White, Colors.Black);
            // Remove it when done
            scene.QueueFree();
        }

        public void PopupNumber(Node3D point, int amount, Color textColor, Color outlineColor)
        {
            // Create scene
            var scene = _PopupLabelScene.Instantiate<PopupLabel>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            // Remove it when done
            scene.Connect(
                nameof(scene.Finished),
                Callable.From(scene.QueueFree),
                (uint)ConnectFlags.OneShot
            );
            scene.PopupNumber(amount, textColor, outlineColor);
        }

        public async Task PopupNumberAsync(Node3D point, int amount, Color textColor, Color outlineColor)
        {
            // Create scene
            var scene = _PopupLabelScene.Instantiate<PopupLabel>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            await scene.PopupNumberAsync(amount, textColor, outlineColor);
            // Remove it when done
            scene.QueueFree();
        }


        public void Alert(Node3D point)
        {
            // Create scene
            var scene = _AlertEffectScene.Instantiate<AlertEffect>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            scene.Connect(
                nameof(scene.Finished),
                Callable.From(scene.QueueFree),
                (uint)ConnectFlags.OneShot
            );
            scene.Popup();
        }

        public async Task AlertAsync(Node3D point)
        {
            // Create scene
            var scene = _AlertEffectScene.Instantiate<AlertEffect>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            await scene.PopupAsync();
            // Remove it when done
            scene.QueueFree();
        }


        public void TalkingEffect(Node3D point)
        {
            // Create scene
            var scene = _TalkingEffectScene.Instantiate<TalkingEffect>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            scene.Connect(
                nameof(scene.Finished),
                Callable.From(scene.QueueFree),
                (uint)ConnectFlags.OneShot
            );
            scene.Popup();
        }

        public async Task TalkingEffectAsync(Node3D point)
        {
            // Create scene
            var scene = _TalkingEffectScene.Instantiate<TalkingEffect>();
            scene.Hide();
            point.AddChild(scene);
            scene.Show();
            await scene.PopupAsync();
            // Remove it when done
            scene.QueueFree();
        }
    }
}
