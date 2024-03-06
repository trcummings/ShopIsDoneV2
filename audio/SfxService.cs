using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Audio
{
    // This is a simple service that just allows any injectable to play SFX from
    // anywhere
    public partial class SfxService : Node, IService
    {
        private Queue<AudioStreamPlayer> _Pool;

        public override void _Ready()
        {
            // Collect all audio stream player children
            _Pool = new Queue<AudioStreamPlayer>(GetChildren().OfType<AudioStreamPlayer>());
        }

        public Command RunPlaySfx(AudioStream stream)
        {
            return new DeferredCommand(() =>
            {
                // Get next player from the queue
                var player = _Pool.Dequeue();
                // Add that same player back to the queue
                _Pool.Enqueue(player);

                // Get the length of the stream
                var length = stream.GetLength();

                // Play the SFX and wait for the duration of the stream to be over
                return new SeriesCommand(
                    new ActionCommand(() => {
                        player.Stream = stream;
                        player.Play();
                    }),
                    new WaitForCommand(this, (float)length)
                );
            });
        }
    }
}

