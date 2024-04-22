using System;
using Godot;
using System.Threading.Tasks;
using Godot.Collections;

namespace ShopIsDone.Microgames
{
	public partial class MicrogameManager : Node
	{
        [Signal]
        public delegate void MicrogameFinishedEventHandler(int outcome);

        [Export]
        private Control _Background;

        [Export]
        private Control _FrameContainer;

        [Export]
        private Viewport _MicrogameViewport;

        [Export]
        private Label _PromptLabel;

        [Export]
        private ProgressBar _ProgressBar;

        [Export]
        private Label _RemainingTicksLabel;

        [Export]
        private Control _ProgressContainer;

        [Export]
        private AudioStreamPlayer _HeartbeatPlayer;

        [Export]
        private AudioStreamPlayer _ClockTickPlayer;

        // State
        private Microgame _Microgame;
        private Tween _ProgressTween;

        public override void _Ready()
        {
            // Hide the pieces of the container
            _FrameContainer.Hide();
            _PromptLabel.Hide();

            // Set Progress to 100%
            _ProgressBar.Value = 100;
            // Hide remaining ticks label
            _RemainingTicksLabel.Hide();
        }

        public async void RunMicrogame(Microgame microgame, MicrogamePayload payload)
		{
            Microgame.Outcomes outcome = Microgame.Outcomes.Loss;

            // Add microgame to the container and hide it
            _Microgame = microgame;
            _MicrogameViewport.AddChild(_Microgame);

            // Connect to microgame signals
            _Microgame.BeatTicked += OnMicrogameProgress;
            _Microgame.MicrogameFinished += OnMicrogameFinished;
            _Microgame.HideTimerRequested += OnRequestTimerHidden;
            _Microgame.ShowTimerRequested += OnRequestTimerShown;
            _Microgame.HideBackgroundRequested += OnRequestBackgroundHidden;
            _Microgame.ShowBackgroundRequested += OnRequestBackgroundShown;

            // Initialize the microgame
            _Microgame.Init(payload);

            // Tween in
            await TweenFrameIn(_Microgame.PromptText);
            // Start the microgame, and return the outcome
            outcome = await StartMicrogame();
            // Tween out
            await TweenFrameOut();

            // Remove microgame from container
            _MicrogameViewport.RemoveChild(_Microgame);
            _Microgame.QueueFree();
            _Microgame = null;

            EmitSignal(nameof(MicrogameFinished), (int)outcome);
		}

		private async Task<Microgame.Outcomes> StartMicrogame()
		{
            // TODO: Reveal the microgame instead of just waiting
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

            // Tween text out
            GetTree().CreateTween()
                .BindNode(this)
                .TweenProperty(_PromptLabel, "modulate:a", 0f, 0.5f)
                // Set ease and trans type
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Linear);

            // Wait until right before text is gone to start
            await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

            // Start microgame
            _Microgame.Start();

            // Wait for microgame to be finished and extract the outcome
            var result = await ToSignal(_Microgame, nameof(_Microgame.MicrogameFinished));
            var outcome = (Microgame.Outcomes)(int)result[0];

            // Wait a moment afterwards to hold on the screen
            await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

            // Return outcome
            return outcome;
        }

		private async Task TweenFrameIn(string promptLabel)
		{
			// Set our rect position off frame by the full vertical size of the rect
			var frameStartPos = _FrameContainer.Position - new Vector2(0, _FrameContainer.Size.Y);
			var frameEndPos = _FrameContainer.Position;

			// Set the frame to be off screen
			_FrameContainer.Position = frameStartPos;

			// Show frame container
			_FrameContainer.Show();

            // Set prompt pivot point to center of label size
            var pivot = _PromptLabel.Size / 2;
            _PromptLabel.PivotOffset = pivot;

            // Hide prompt by setting modulate to zero
			_PromptLabel.Modulate = _PromptLabel.Modulate with { A = 0 };

            // "Show" prompt now that it's transparent
            _PromptLabel.Show();

			// Set prompt label
			_PromptLabel.Text = promptLabel;

			// Scale prompt
			_PromptLabel.Scale = new Vector2(8, 8);

            // Create tween
            var tween = GetTree().CreateTween().BindNode(this);

			// Tween Frame in
            tween
                .TweenProperty(_FrameContainer, "position", frameEndPos, 0.5f)
				// Set ease and trans type
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Bounce);

			// In parallel, tween prompt
			// Transparency
			tween
                .Parallel()
				.TweenProperty(_PromptLabel, "modulate:a", 1f, 0.35f)
                // Set ease and trans type
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Expo);

			// Scale
            tween
                .Parallel()
				.TweenProperty(_PromptLabel, "scale", new Vector2(1f, 1f), 0.5f)
                // Set ease and trans type
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Elastic);

			// Await finish
            await ToSignal(tween, "finished");
        }

		private async Task TweenFrameOut()
		{
            // Set our rect position off frame by the full vertical size of the rect
            var frameEndPos = _FrameContainer.Position - new Vector2(0, _FrameContainer.Size.Y);
            var frameStartPos = _FrameContainer.Position;

            // Tween out background
            if (_Background.Modulate != Colors.Transparent) OnRequestBackgroundHidden(0.25f);

            // Create tween
            var tween = GetTree().CreateTween();

            // Tween Frame out
            tween
                .BindNode(this)
                .TweenProperty(_FrameContainer, "position", frameEndPos, 0.5f)
                // Set ease and trans type
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Bounce);

            // Await finish
            await ToSignal(tween, "finished");

			// Kill tween
			tween.Kill();

            // Reposition frame container to normal
            _FrameContainer.Position = frameStartPos;

            // Hide frame container
            _FrameContainer.Hide();

			// Hide label
			_PromptLabel.Hide();

            // Reset label modulate
            _PromptLabel.Modulate = _PromptLabel.Modulate with { A = 1 };

            // Reshow timer
            _ProgressContainer.Modulate = Colors.White;
            // Set Progress to 100%
            _ProgressBar.Value = 100;
            // Hide remaining ticks label
            _RemainingTicksLabel.Hide();
        }

        private void OnMicrogameProgress(int currentBeat)
        {
            // Calculate remaining beats
            var remainingBeats = _Microgame.NumBeats - currentBeat;
            // Update label
            _RemainingTicksLabel.Text = remainingBeats.ToString();

            // On final beat, return early
            if (currentBeat == _Microgame.NumBeats) return;

            // Play a heartbeat each even beat
            else if (currentBeat % 2 == 0)
            {
                _HeartbeatPlayer.PitchScale = (_Microgame.BPM / 2f) / 60;
                _HeartbeatPlayer.Play();
            }

            // With four beats left, play the clock ticking and show label
            if (remainingBeats <= 3)
            {
                // Noises
                _ClockTickPlayer.PitchScale = _Microgame.BPM / 60;
                _ClockTickPlayer.Play();
                // Label
                _RemainingTicksLabel.Show();
            }

            // On first beat just make sure we're set to max
            if (currentBeat == 0)
            {
                _ProgressBar.Value = 100;
            }

            // Increment current beat by 1 so we're ahead of the final beat
            var goalValue = (1f - ((currentBeat + 1) / (float)_Microgame.NumBeats)) * 100;
            var duration = _Microgame.GetSecondsPerBeat();
            // Create tween and run it
            _ProgressTween = GetTree().CreateTween().BindNode(this);
            _ProgressTween
                .TweenProperty(_ProgressBar, "value", goalValue, duration)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Expo);
        }

        private void OnMicrogameFinished(Microgame.Outcomes _)
        {
            if (_HeartbeatPlayer.Playing) _HeartbeatPlayer.Stop();
            if (_ClockTickPlayer.Playing) _ClockTickPlayer.Stop();
            if (_ProgressTween != null)
            {
                _ProgressTween.Kill();
                _ProgressTween = null;
            }
        }

        private void OnRequestTimerHidden(float duration)
        {
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_ProgressContainer, "modulate:a", 0, duration);
        }

        private void OnRequestTimerShown(float duration)
        {
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_ProgressContainer, "modulate:a", 1, duration);
        }

        private async void OnRequestBackgroundHidden(float duration)
        {
            var tween = GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_Background, "modulate:a", 0, duration);
            await ToSignal(tween, "finished");

            // Hide the background afterwards but reset its modulate value
            _Background.Hide();
            _Background.Modulate = Colors.White;
        }

        private void OnRequestBackgroundShown(float duration)
        {
            // Make the background transparent and show it before tweening
            _Background.Modulate = Colors.Transparent;
            _Background.Show();

            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_Background, "modulate:a", 1, duration);
        }
    }
}

