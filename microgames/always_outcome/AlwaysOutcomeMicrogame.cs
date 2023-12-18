using System;
using Godot;

namespace Microgames
{
    public partial class AlwaysOutcomeMicrogame : Microgame
	{
        [Export(PropertyHint.Enum, "Loss, Win")]
        public int AlwaysOutcome = 0;

        // Nodes
        private Label _Label;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Label = GetNode<Label>("%Label");
        }

        public override void Start()
        {
            // Give this thing half the number of beats
            NumBeats = 5;

            // Base call
            base.Start();

            // Set outcome to win
            Outcome = (Outcomes)AlwaysOutcome;
            // Set label
            _Label.Text = Outcome == Outcomes.Loss
                ? "Always Lose"
                : "Always Win";
        }

        protected override void OnTimerFinished()
        {
            if (Outcome == Outcomes.Loss) PlayFailureSfx();
            else if (Outcome == Outcomes.Win) PlaySuccessSfx();
        }
    }
}

