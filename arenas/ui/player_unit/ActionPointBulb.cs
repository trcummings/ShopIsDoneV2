using Godot;
using System;

namespace ShopIsDone.Arenas.UI
{
    [Tool]
    public partial class ActionPointBulb : Control
    {
        [Export]
        public BulbStates BulbState
        {
            get { return _BulbState; }
            set { SetBulbState(value); }
        }
        private BulbStates _BulbState = BulbStates.BurnedOut;
        public enum BulbStates
        {
            BurnedOut = 0,
            Debt = 1,
            Lit = 2,
            Excess = 3
        }

        // Nodes
        private Sprite2D _BulbSprite;

        public override void _Ready()
        {
            // Ready nodes
            _BulbSprite = GetNode<Sprite2D>("%Sprite");
        }

        public void SetBulbState(BulbStates state)
        {
            // Guarantee bulbsprite in editor
            if (_BulbSprite == null) _BulbSprite = GetNode<Sprite2D>("%Sprite");

            // Set frame
            _BulbSprite.Frame = (int)state;

            // Set state
            _BulbState = state;
        }
    }
}

