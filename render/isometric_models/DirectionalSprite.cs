using System;
using Godot;

namespace ShopIsDone.Models.IsometricModels
{
    [Tool]
    public partial class DirectionalSprite : Node2D
    {
        [Export]
        public int ActionFrame
        {
            get { return _ActionFrame; }
            set { SetActionFrame(value); }
        }
        private int _ActionFrame = 0;

        [Export]
        public string Direction
        {
            get { return _Direction; }
            set { SetDirection(value); }
        }
        private string _Direction = "forwardright";

        [Export]
        private Sprite2D _Sprite;

        private void SetActionFrame(int value)
        {
            // If there's an update
            if (value != _ActionFrame)
            {
                _ActionFrame = value;
                SetSpriteFrame();
            }
        }

        private void SetDirection(string value)
        {
            // If there's an update
            if (!string.IsNullOrEmpty(value) && value != _Direction)
            {
                _Direction = value;
                SetSpriteFrame();
            }
        }

        private void SetSpriteFrame()
        {
            // Get row
            var row = 0;
            if (_Direction == "forwardleft") row = 0;
            else if (_Direction == "forwardright") row = 1;
            else if (_Direction == "backleft") row = 2;
            else if (_Direction == "backright") row = 3;

            // Set proper frame
            if (_Sprite != null) _Sprite.Frame = _ActionFrame + (row * _Sprite.Hframes);
        }
    }


}

