using System;
using Godot;
using Godot.Collections;
using System.Linq;
using SystemGenerics = System.Collections.Generic;

namespace ShopIsDone.Microgames.HallwayChase
{
	public partial class FootstepController : Node
	{
        public enum FootSpeed
        {
            Slow,
            Medium,
            Fast
        }

		[Export]
		public Array<AudioStream> SlowSteps = new Array<AudioStream>();

        [Export]
        public Array<AudioStream> MediumSteps = new Array<AudioStream>();

        [Export]
        public Array<AudioStream> FastSteps = new Array<AudioStream>();

        private SystemGenerics.Queue<AudioStreamPlayer> _FootfallPlayers;

        public override void _Ready()
        {
            _FootfallPlayers = new SystemGenerics.Queue<AudioStreamPlayer>(GetChildren().OfType<AudioStreamPlayer>());
        }

        private float _FootTimer = 0.0f;

        public void ProcessFootsteps(bool isMovingOnGround, FootSpeed speed, float delta)
        {
            // Reset foot timer when we're not moving
            if (!isMovingOnGround) _FootTimer = 0.0f;
            // Otherwise increment
            else _FootTimer += delta;

            // If we've reached a step at our given speed, play a footstep
            if (_FootTimer >= SpeedToTime(speed))
            {
                // Reset timer
                _FootTimer = 0.0f;
                // Get next player, then add that player to the back of the queue
                var player = _FootfallPlayers.Dequeue();
                _FootfallPlayers.Enqueue(player);

                player.PitchScale = (float)GD.RandRange(0.8, 1.2);
                player.Stream = PickFromSteps(speed);
                player.Play();
            }
        }

        private float SpeedToTime(FootSpeed speed)
        {
            switch (speed)
            {
                case FootSpeed.Slow: return 0.65f;
                case FootSpeed.Medium: return 0.5f;
                case FootSpeed.Fast: return 0.35f;
                default: return 0.35f;
            }
        }

        private AudioStream PickFromSteps(FootSpeed speed)
        {
            switch (speed)
            {
                case FootSpeed.Slow: return SlowSteps.PickRandom();
                case FootSpeed.Medium: return MediumSteps.PickRandom();
                case FootSpeed.Fast: return FastSteps.PickRandom();
                default: return SlowSteps.PickRandom();
            }
        }
    }
}

