using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Microgames.PoseMannequin
{
    public partial class ApproachingMannequinState : State
    {
        [Export]
        private Camera3D _Camera;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Make this camera the current camera
            _Camera.MakeCurrent();
        }
    }
}

