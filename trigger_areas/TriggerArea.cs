using Godot;
using System;

namespace ShopIsDone.TriggerAreas
{
    // This is a class for triggering anything when the area is entered,
    // providing a niceity for one-shot triggers, but is meant to be inherited
    // which is why it's marked abstract
    [Tool]
    public abstract partial class TriggerArea : Area3D
    {
        [Signal]
        public delegate void AreaTriggeredEventHandler();

        [Export]
        private bool _DisableOnEntered = true;

        public override void _Ready()
        {
            if (_DisableOnEntered)
            {
                BodyEntered += (Node3D _) =>
                {
                    DisableMonitoring();
                    DisableMonitorable();
                };
                AreaEntered += (Area3D _) =>
                {
                    DisableMonitoring();
                    DisableMonitorable();
                };
            }
        }

        protected void Trigger()
        {
            EmitSignal(nameof(AreaTriggered));
        }

        protected void EnableMonitoring()
        {
            SetDeferred("monitoring", true);
        }

        protected void EnableMonitorable()
        {
            SetDeferred("monitorable", true);
        }

        protected void DisableMonitoring()
        {
            SetDeferred("monitoring", false);
        }

        protected void DisableMonitorable()
        {
            SetDeferred("monitorable", false);
        }
    }
}

