using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Utils;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class BaseCustomer : Node2D, IStoppable
    {
        [Signal]
        public delegate void LeftEventHandler();

        [Signal]
        public delegate void ArrivedAtSaladBarEventHandler();

        private AnimatedSprite2D _AnimatedSprite;

        private StateMachine _FSM;

        public override void _Ready()
        {
            base._Ready();
            _AnimatedSprite = GetNode<AnimatedSprite2D>("%AnimatedSprite");
            _FSM = GetNode<StateMachine>("%StateMachine");
        }

        public void Start(Vector2 destination)
        {
            _FSM.ChangeState("MovingState", new Dictionary<string, Variant>()
            {
                { "Destination", destination }
            });
        }

        public void Stop()
        {
            _FSM.SetProcess(false);
            _FSM.SetPhysicsProcess(false);
        }

        public virtual async void Leave()
        {
            _AnimatedSprite.Play("disappear");
            await ToSignal(_AnimatedSprite, "animation_finished");
            EmitSignal(nameof(Left));
            QueueFree();
        }
    }
}
