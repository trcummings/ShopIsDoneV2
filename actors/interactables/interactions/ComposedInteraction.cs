using System;
using System.Linq;
using Godot;
using System.Collections.Generic;

namespace ShopIsDone.Interactables.Interactions
{
    // A composed interaction gets all the interactions that are children of
    // this interaction and runs them in series
    [Tool]
    public partial class ComposedInteraction : Interaction
	{
        private Queue<Interaction> _Interactions = new Queue<Interaction>();
        private Interaction _CurrentInteraction;

        public override void _Ready()
        {
            base._Ready();
            _Interactions = new Queue<Interaction>(GetChildren().OfType<Interaction>());
        }

        public override void Execute()
        {
            // If no interactions, just emit and end
            if (_Interactions.Count == 0)
            {
                CallDeferred(nameof(Finish));
                return;
            }

            // Start interaction cycle
            RunNextInteraction();
        }

        private void OnInteractionFinished()
        {
            // Disconnect
            if (_CurrentInteraction != null)
            {
                _CurrentInteraction.Finished -= OnInteractionFinished;
            }

            // Check length of candidate commands, if empty, emit finished
            if (_Interactions.Count == 0)
            {
                Finish();
                _CurrentInteraction = null;
            }
            // Otherwise, start the cycle with the next command
            else RunNextInteraction();
        }

        private void RunNextInteraction()
        {
            // Pop off first element
            _CurrentInteraction = _Interactions.Dequeue();

            // Connect to its finished signal
            _CurrentInteraction.Finished += OnInteractionFinished;

            // HACK: This runs some kind of effect that forces the connections to stay
            // in memory so we have no problem chaining async commands together
            var _ = _CurrentInteraction.GetSignalConnectionList(nameof(Finished));

            // Run it
            _CurrentInteraction.Execute();
        }
    }
}

