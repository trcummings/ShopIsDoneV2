using System;
using Godot;
using ShopIsDone.Game;
using ShopIsDone.Utils;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Levels
{
	public partial class LevelRngService : Node, IService, IInitializable
    {
        [Export]
        private bool _Debug = false;

		private RandomNumberGenerator _RNG;

        // TODO: Load in from a seed on level start
		public void Init()
		{
			_RNG = new RandomNumberGenerator();
		}

        public bool PercentCheck(float value)
        {
            if (value > 1f)
            {
                GD.PrintErr($"Given value of \"{value}\" for PercentCheck exceeds 100%! Check will always pass");
            }

            // Roll a random float and check if we've exceeded it
            var rollValue = _RNG.Randf();
            var succeded = value > rollValue;

            // Debug print output
            if (GameManager.IsDebugMode() && _Debug)
            {
                var outcomeStr = succeded ? "Success" : "Failure";
                GD.Print($"Running \"PercentCheck\" from {Name} with {value} against {rollValue}. {outcomeStr}");
            }

            return succeded;
        }

        public static bool RandomBool()
        {
            return (new RandomNumberGenerator().Randi() & 2) -1 == 1;
        }
    }
}

