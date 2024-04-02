using Godot;
using ShopIsDone.Levels;
using System;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class ShamblerHand : Grabber
    {
        [Signal]
        public delegate void DrainedEventHandler(float amount);

        [Export]
        public float SecondsToDrain = 5;

        private Sprite2D _Sprite;

        public int NumSlaps = 0;
        private float _DrainPerFrame;
        private bool _IsGrabbing = false;

        public override void _Ready()
        {
            base._Ready();
            _Sprite = GetNode<Sprite2D>("%Sprite");
            _Sprite.FlipH = LevelRngService.RandomBool();
            // Calculate drain per frame
            _DrainPerFrame = 1 / (60 * SecondsToDrain);
        }

        public override void ReceiveSlap()
        {
            NumSlaps += 1;
            EmitSignal(nameof(Slapped));
            if (NumSlaps >= 3) _AnimPlayer.Play("hurt");
        }

        public override void Stop()
        {
            _IsGrabbing = false;
            base.Stop();
        }

        protected override void OnGrabberReachedTarget()
        {
            // Play grab animation
            _AnimPlayer.Play("grab");
            // Set grabbing state to true
            _IsGrabbing = true;
        }

        public override void _PhysicsProcess(double _)
        {
            // Ignore if not grabbing
            if (!_IsGrabbing) return;
            // Drain health
            EmitSignal(nameof(Drained), _DrainPerFrame);
        }

        public override void Withdraw()
        {
            // Set grabbing state to false
            _IsGrabbing = false;
            base.Withdraw();
        }
    }
}
