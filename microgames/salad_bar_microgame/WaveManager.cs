using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Microgames.SaladBar
{
    // This class handles sequencing "waves" of animations that spawn challenges
    // over time
    public partial class WaveManager : Node
    {
        [Signal]
        public delegate void WaveFinishedEventHandler();

        [Signal]
        public delegate void AllWavesFinishedEventHandler();

        [Export]
        private AnimationPlayer _WavePlayer;

        // State
        private Queue<string> _WaveNames = new Queue<string>();

        // This tracks whether or not the animation player has finished the current
        // wave's sequence
        private bool _IsPlayerFinished = false;

        // This tracks whether or not the wave has been finished externally
        private bool _IsWaveFinished = false;

        public override void _Ready()
        {
            // Gather all waves
            _WaveNames = new Queue<string>(_WavePlayer.GetAnimationList().Where(s => s.StartsWith("wave_")));
            // Connect animation player
            _WavePlayer.Connect("animation_finished", new Callable(this, nameof(OnWaveAnimationFinished)));
        }

        public void StartNextWave()
        {
            // If no waves left, end
            if (_WaveNames.Count == 0)
            {
                EmitSignal(nameof(AllWavesFinished));
                return;
            }

            // Pull the next wave name, and start it
            var nextWave = _WaveNames.Dequeue();
            _WavePlayer.Play(nextWave);
        }

        public void FinishCurrentWave()
        {
            _IsWaveFinished = true;
            OnWaveFinished();
        }

        public void Stop()
        {
            _WavePlayer.Stop();
        }

        private void OnWaveFinished()
        {
            // If the player has finished and the wave has finished externally
            if (_IsPlayerFinished && _IsWaveFinished)
            {
                // Emit signal
                EmitSignal(nameof(WaveFinished));

                // Reset the values
                _IsWaveFinished = false;
                _IsPlayerFinished = false;

                // Start the next wave
                StartNextWave();
            }
        }

        private void OnWaveAnimationFinished(string _)
        {
            _IsPlayerFinished = true;
            OnWaveFinished();
        }
    }
}
