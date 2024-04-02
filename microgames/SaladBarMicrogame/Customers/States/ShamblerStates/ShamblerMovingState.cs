using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Microgames.SaladBar.States
{
    public partial class ShamblerMovingState : MovingState
    {
        [Export]
        private AudioStreamPlayer _ApproachPlayer;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Start playing shambler audio
            _ApproachPlayer.VolumeDb = Mathf.DbToLinear(-80);
            _ApproachPlayer.Play();
            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);
            // Make audio louder as we get closer
            var distance = _Customer.GlobalPosition.DistanceTo(_Destination);
            var total = _Destination.DistanceTo(_StartPos);
            var linear = 1 - distance / total;
            _ApproachPlayer.VolumeDb = Mathf.LinearToDb(linear);
        }
    }
}
