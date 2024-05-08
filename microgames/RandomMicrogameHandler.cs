using Godot;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;

namespace ShopIsDone.Microgames
{
	public partial class RandomMicrogameHandler : MicrogameHandler, IOnCleanupComponent
	{
		[Export]
		public Array<PackedScene> AllMicrogames;
		private PackedScene _NextScene;

        public override void Init()
        {
            base.Init();
			PickNextMicrogame();
        }

        public Command OnCleanup()
		{
			return new ActionCommand(PickNextMicrogame);
		}

        protected override Microgame GetMicrogame(MicrogamePayload _)
        {
			return _NextScene.Instantiate<Microgame>();
        }

        private void PickNextMicrogame()
		{
            _NextScene = AllMicrogames.PickRandom(); 
		}
	}
}