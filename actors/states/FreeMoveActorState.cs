﻿using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Interactables;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Actors.States
{
	public partial class FreeMoveActorState : State
    {
        [Export]
        private CharacterBody3D _Body;

        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorFloorIndicator _ActorFloorIndicator;

        [Export]
        private InteractableHandler _InteractableHandler;

        private IActorInput _ActorInput = new ActorInput();

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Get actor input from message
            var input = (IActorInput)(GodotObject)message[Consts.INPUT_KEY];
            _ActorInput = input;

            // Show floor indicator
            _ActorFloorIndicator.Show();

            // Activate interactable handler
            _InteractableHandler.Activate();

            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            if (Input.IsActionJustPressed("ui_accept")) _InteractableHandler.Interact();

            _ActorInput.UpdateInput();
            _ActorVelocity.AccelerateInDirection(_ActorInput.MoveDir);
            _ActorVelocity.Move(_Body);
            _ActorAnimator.UpdateAnimations(_ActorVelocity.Velocity);
            _ActorFloorIndicator.UpdateIndicator(_ActorVelocity.Velocity);
        }

        public override void OnExit(string nextState)
        {
            // Hide floor indicator
            _ActorFloorIndicator.Hide();

            // Deactivate interactable handler
            _InteractableHandler.Deactivate();

            base.OnExit(nextState);
        }
    }
}

