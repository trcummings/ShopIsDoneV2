using Godot;
using System;

namespace ShopIsDone.UI
{
    public interface IVerticalDrawerChild
    {
        Control DrawerFace { get; }
        Vector2 Size { get; }
        Vector2 Position { get; set; }

        void Show();
        void Hide();
    }

    public partial class VerticalDrawer : Control
    {
        [Export]
        public NodePath DrawerPath;

        private IVerticalDrawerChild _Drawer;
        private Tween _DrawerTween;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _Drawer = GetNode<IVerticalDrawerChild>(DrawerPath);
        }

        public void InitDrawer()
        {
            // Position rules UI above top of screen
            _Drawer.Show();
            _Drawer.Position = _Drawer.Position with { Y = -_Drawer.Size.Y };
        }

        public void HideDrawer()
        {
            // Position drawer above top of screen
            _Drawer.Position = _Drawer.Position with { Y = -_Drawer.Size.Y };
            _Drawer.Hide();
        }

        public void Activate()
        {
            PushIn();
        }

        public void Deactivate()
        {
            // Retract to above screen
            _DrawerTween?.Kill();
            _DrawerTween = GetTree().CreateTween();
            _DrawerTween.TweenProperty(_Drawer as Control, "position:y", -_Drawer.Size.Y, .15f);
        }

        public void PullOut()
        {
            _DrawerTween?.Kill();
            _DrawerTween = GetTree().CreateTween();
            _DrawerTween.TweenProperty(_Drawer as Control, "position:y", 0, .15f);
        }

        public void PushIn()
        {
            // Bring out to retracted point
            _DrawerTween?.Kill();
            _DrawerTween = GetTree().CreateTween();
            _DrawerTween.TweenProperty(_Drawer as Control, "position:y", GetDrawerRetractedPoint(), .15f);
        }

        private float GetDrawerRetractedPoint()
        {
            // At the point where the drawer face is just below the screen top 
            var faceY = _Drawer.DrawerFace.Size.Y;
            var totalY = _Drawer.Size.Y;

            return -(totalY - faceY);
        }
    }
}

