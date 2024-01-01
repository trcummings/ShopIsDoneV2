using System;
using Godot;
using ShopIsDone.EntityStates;
using Godot.Collections;

namespace ShopIsDone.Entities.PuppetCustomers.States
{
    public partial class IntimidateEntityState : EntityState
    {
        [Export]
        public AudioStreamPlayer _IntimidatePlayer;

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            _IntimidatePlayer.Connect(
                "finished",
                Callable.From(() => EmitSignal(nameof(StateEntered))),
                (uint)ConnectFlags.OneShot
            );
            _IntimidatePlayer.Play();
        }

        public override bool IsInArena()
        {
            return true;
        }

        public override bool CanAct()
        {
            return true;
        }
    }
}

