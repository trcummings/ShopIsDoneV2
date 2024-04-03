using System;
using Godot;

namespace ShopIsDone.Microgames.SaladBar
{
	public partial class SaladBarEvents : Resource
	{
		[Signal]
		public delegate void NastyHandRequestedEventHandler();

        [Signal]
        public delegate void TongsRequestedEventHandler();

        [Signal]
        public delegate void ShamblerHandRequestedEventHandler();

        [Signal]
        public delegate void NastyHandSpawnedEventHandler(Grabber hand);

        [Signal]
        public delegate void TongsSpawnedEventHandler(Grabber tongs);

        [Signal]
        public delegate void ShamblerHandSpawnedEventHandler(Grabber shamblerHand);
    }
}

