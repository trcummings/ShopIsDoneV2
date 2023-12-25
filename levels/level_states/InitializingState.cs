using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Actors;

namespace ShopIsDone.Levels.States
{
    public partial class InitializingState : State
    {
        [Export]
        private Node3D _DefaultSpawnPoint;

        [Export]
        private Haskell _Haskell;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Move actor to default spawn position
            _Haskell.GlobalTransform = _DefaultSpawnPoint.GlobalTransform;

            // Finish the start hook
            base.OnStart(message);

            // Go to the free move state for now
            ChangeState("FreeMoveState");
        }
    }
}
