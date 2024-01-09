using Godot;
using System;

namespace ShopIsDone.WorldEnvironments
{
    public partial class EnvironmentManager : WorldEnvironment
    {
        public override void _Ready()
        {
            base._Ready();
            // Connect to global events
            var events = Events.GetEvents(this);
            events.ChangeEnvironmentRequested += OnChangeEnvironment;
        }

        private void OnChangeEnvironment(Godot.Environment environment)
        {
            Environment = environment;
        }
    }
}
