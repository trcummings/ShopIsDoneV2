using System;
using Godot.Collections;
using Godot;
using ShopIsDone.Utils.StateMachine;
using SystemGenerics = System.Collections.Generic;

namespace ShopIsDone.Microgames.SaladBar.States
{
	public partial class AtSaladBarState : State
    {
        [Signal]
        public delegate void ActionDelayTimeoutEventHandler();

        [Signal]
        public delegate void WaveDelayTimeoutEventHandler();

        [Export]
        private NodePath _CustomerPath;
        protected BaseCustomer _Customer;

        [Export]
        protected Timer _WaveDelayTimer;

        [Export]
        protected Timer _ActionDelayTimer;

        [Export]
        public SaladBarEvents Events;

        // Time in seconds in between "waves" of grabbing
        [Export]
        public float WaveDelay = 5;

        // Time in seconds between spawning each hand or tong
        [Export]
        public float ActionDelay = 0.15f;

        // State
        protected Array<Grabber> _Grabbers = new Array<Grabber>();
        protected SystemGenerics.Queue<Action> _Actions = new SystemGenerics.Queue<Action>();
        protected int _NumActionWaves = 0;

        public override void _Ready()
        {
            base._Ready();
            _Customer = GetNode<BaseCustomer>(_CustomerPath);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Init timers
            _ActionDelayTimer.WaitTime = ActionDelay;
            _WaveDelayTimer.WaitTime = WaveDelay;

            // Connect to signals
            _ActionDelayTimer.Timeout += RunNextAction;
            _WaveDelayTimer.Timeout += StartNextActionWave;

            // Connect to Events
            Events.NastyHandSpawned += OnGrabberSpawned;
            Events.TongsSpawned += OnGrabberSpawned;
            Events.ShamblerHandSpawned += OnGrabberSpawned;

            // Decide next action
            StartNextActionWave();
        }

        protected void StartNextActionWave()
        {
            _NumActionWaves += 1;
            // Clear actions
            _Actions.Clear();

            // If we're satiated, leave
            if (ShouldLeave())
            {
                // Leave
                _Customer.Leave();
                // Return early
                return;
            }

            // Gather actions
            GatherActions();

            // Run next action
            RunNextAction();
        }

        protected void RunNextAction()
        {
            // If we're satiated, leave
            if (ShouldLeave())
            {
                // Leave
                _Customer.Leave();
                // Return early
                return;
            }

            // If we're out of actions, start the next action wave
            if (_Actions.Count == 0)
            {
                _WaveDelayTimer.Start();
                return;
            }

            // Otherwise, dequeue next action and start action timer
            var nextAction = _Actions.Dequeue();
            nextAction?.Invoke();
            _ActionDelayTimer.Start();
        }

        protected virtual bool ShouldLeave()
        {
            // Override here
            return true;
        }

        protected virtual void GatherActions()
        {
            // Override here
        }

        protected virtual void OnGrabberSpawned(Grabber grabber)
        {
            _Grabbers.Add(grabber);
            // Connect to its events
            grabber.Grabbed += () => OnGrabbedFood(grabber);
            grabber.Slapped += () => OnSlapped(grabber);
        }

        protected virtual void OnGrabbedFood(Grabber _)
        {
            // Override here
        }

        protected virtual void OnSlapped(Grabber _)
        {
            // Override here
        }
    }
}

