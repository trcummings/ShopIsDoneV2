using System;
using Godot;

namespace ShopIsDone.Audio
{
    public partial class FootstepsController : Node
    {
        public enum FootSpeed
        {
            Slow = 0,
            Medium = 1,
            Fast = 2
        }

        [Export]
        private AudioStreamPlayer _SlowPlayer;

        [Export]
        private AudioStreamPlayer _MediumPlayer;

        [Export]
        private AudioStreamPlayer _FastPlayer;

        // State
        private AudioStreamPlayer _CurrentPlayer;
        private FootSpeed _CurrentSpeed = FootSpeed.Slow;

        public override void _Ready()
        {
            base._Ready();
            _CurrentPlayer = _SlowPlayer;
        }

        public void PlayFootstep()
        {
            _CurrentPlayer.Play();
        }

        public void SetFootSpeed(FootSpeed speed = FootSpeed.Slow)
        {
            switch (speed)
            {
                case FootSpeed.Slow:
                    {
                        _CurrentPlayer = _SlowPlayer;
                        break;
                    }
                case FootSpeed.Medium:
                    {
                        _CurrentPlayer = _MediumPlayer;
                        break;
                    }
                case FootSpeed.Fast:
                    {
                        _CurrentPlayer = _FastPlayer;
                        break;
                    }
                default:
                    {
                        _CurrentPlayer = _SlowPlayer;
                        break;
                    }
            }
        }
    }
}
