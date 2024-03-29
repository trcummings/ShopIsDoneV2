using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Microgames
{
    public partial class ChooseOutcomeMicrogame : Microgame
    {
        [Signal]
        public delegate void SelectedEventHandler();

        // Nodes
        private Label _Label;
        private Button _Win;
        private Button _Lose;
        private Button _SelectedButton;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Label = GetNode<Label>("%Label");
            _Win = GetNode<Button>("%Win");
            _Lose = GetNode<Button>("%Lose");

            // Pause processing
            SetProcess(false);
        }

        public override void Init(Dictionary<string, Variant> msg)
        {
            // Connect
            _Win.Pressed += OnButtonPressed;
            _Lose.Pressed += OnButtonPressed;

            // Focus win button
            FocusButton(_Win);
        }

        public override void Start()
        {
            // Give this thing half the number of beats
            NumBeats = 5;

            // Base call
            base.Start();

            // Start processing
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            // Movement input
            if (
                Input.IsActionJustPressed("move_left") ||
                Input.IsActionJustPressed("move_right")
            )
            {
                // Emit
                EmitSignal(nameof(Selected));

                // Change selection
                if (_SelectedButton == _Win) FocusButton(_Lose);
                else FocusButton(_Win);
            }
        }

        protected override void OnTimerFinished()
        {
            // Stop all player input
            SetProcess(false);

            if (Outcome == Outcomes.Loss) PlayFailureSfx();
            else if (Outcome == Outcomes.Win) PlaySuccessSfx();

            // Emit
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private void OnButtonPressed()
        {
            _Win.Pressed -= OnButtonPressed;
            _Lose.Pressed -= OnButtonPressed;
            FinishEarly();
        }

        private void FocusButton(Button button)
        {
            _SelectedButton = button;
            // Focus win button
            button.CallDeferred("grab_focus");
            // Set outcome
            if (button == _Win) Outcome = Outcomes.Win;
            else Outcome = Outcomes.Loss;
        }
    }
}
