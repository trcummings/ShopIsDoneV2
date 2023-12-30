using System;
using Godot;
using ShopIsDone.EntityStates;

namespace ShopIsDone.Entities.PuppetCustomers.States
{
    public partial class IntimidateEntityState : EntityState
    {
        [Export]
        public AudioStreamPlayer _IntimidatePlayer;

        public override void Enter()
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

