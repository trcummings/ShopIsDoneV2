using Godot;
using System;
using ShopIsDone.Core;
using Godot.Collections;

namespace ShopIsDone.EntityStates
{
    // Entity states are EXCLUSIVE states that provide information about the entity's
    // presence in the arena and availability and run functions on hooks.
    // Generally speaking, EntityState hooks only show animations or other visual
    // flourishes
    public partial class EntityState : Node
    {
        [Signal]
        public delegate void StateEnteredEventHandler();

        [Signal]
        public delegate void StateExitedEventHandler();

        [Export]
        public string Id;

        [Export]
        private bool _IsActive = true;
        public bool IsActive
        {
            get { return GetIsActive(); }
            private set { _IsActive = value; }
        }

        [Export]
        private bool _IsInArena = true;
        public bool IsInArena
        {
            get { return GetIsInArena(); }
            private set { _IsInArena = value; }
        }

        private LevelEntity _Entity;
        private EntityStateHandler _StateHandler;

        public virtual void Init(LevelEntity entity, EntityStateHandler stateHandler)
        {
            _Entity = entity;
            _StateHandler = stateHandler;
        }

        public virtual void Enter(Dictionary<string, Variant> message = null)
        {
            CallDeferred("emit_signal", nameof(StateEntered));
        }

        public virtual void Exit()
        {
            CallDeferred("emit_signal", nameof(StateExited));
        }

        // Sandboxed methods
        protected virtual bool GetIsInArena()
        {
            return _IsInArena;
        }

        protected virtual bool GetIsActive()
        {
            return _IsActive;
        }
    }
}