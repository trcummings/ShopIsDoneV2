using System;
using Godot;

namespace ShopIsDone.TriggerAreas
{
    // This trigger area triggers the signal immediately
    [Tool]
    public partial class ImmediateTriggerArea : TriggerArea
	{
        public override void _Ready()
        {
            base._Ready();
            BodyEntered += (Node3D _) => Trigger();
            AreaEntered += (Area3D _) => Trigger();
        }
    }
}

