using Godot;
using System;
using ShopIsDone.Core;

namespace ShopIsDone.EntityStates
{
    // Entity states are EXCLUSIVE states that provide information about the entity's
    // presence in the arena and availability and run actions on hooks
    public partial class EntityState : Node
    {
        [Signal]
        public delegate void StateEnteredEventHandler();

        [Signal]
        public delegate void StateExitedEventHandler();

        [Export]
        public string Id;

        private LevelEntity _Entity;
        private EntityStateHandler _StateHandler;

        public virtual void Init(LevelEntity entity, EntityStateHandler stateHandler)
        {
            _Entity = entity;
            _StateHandler = stateHandler;
        }

        public virtual void Enter()
        {
            CallDeferred("emit_signal", nameof(StateEntered));
        }

        public virtual void Exit()
        {
            CallDeferred("emit_signal", nameof(StateExited));
        }

        // Default to false
        public virtual bool IsInArena()
        {
            return false;
        }

        // Default to false
        public virtual bool CanAct()
        {
            return false;
        }
    }
}