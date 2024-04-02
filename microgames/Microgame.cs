using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Microgames
{
    public partial class Microgame : Node
    {
        [Signal]
        public delegate void MicrogameFinishedEventHandler(Outcomes outcome);

        [Signal]
        public delegate void BeatTickedEventHandler(int beat);

        [Signal]
        public delegate void HideTimerRequestedEventHandler(float duration);

        [Signal]
        public delegate void ShowTimerRequestedEventHandler(float duration);

        [Signal]
        public delegate void ShowBackgroundRequestedEventHandler(float duration);

        [Signal]
        public delegate void HideBackgroundRequestedEventHandler(float duration);

        public enum Outcomes
        {
            Loss = 0,
            Win = 1
        }

        [Export]
        public string PromptText = "Help me!";

        [Export]
        public string WidgetText = "Can you help me?";

        // Microgame Timing
        public int NumBeats = 8;
        public float BPM = 120;

        // This is the width/height of the whole thing
        protected const int _Dim = 940;

        // State
        protected Timer MicrogameTimer;
        protected Outcomes Outcome = Outcomes.Loss;
        protected int CurrentBeat = 0;

        // Nodes
        private AudioStreamPlayer _SuccessSfx;
        private AudioStreamPlayer _FailureSfx;

        public override void _Ready()
        {
            _SuccessSfx = GetNode<AudioStreamPlayer>("%SuccessSfxPlayer");
            _FailureSfx = GetNode<AudioStreamPlayer>("%FailureSfxPlayer");

            // Create timer with seconds per beat and connect
            MicrogameTimer = GetNode<Timer>("%MicrogameTimer");
            MicrogameTimer.WaitTime = GetSecondsPerBeat();
            MicrogameTimer.Timeout += OnBeatTimerTick;
        }

        public virtual void Init(MicrogamePayload payload)
        {

        }

        public virtual void Start()
        {
            // Start timer
            MicrogameTimer.Start();

            // Emit first beat
            OnBeat(CurrentBeat);
            EmitSignal(nameof(BeatTicked), 0);
        }

        public float GetSecondsPerBeat()
        {
            // Get beats per second by diving BPM by 60, then invert the fraction
            return 1f / (BPM / 60f);
        }

        // Hooks
        protected virtual void OnTimerFinished()
        {

        }

        protected virtual void OnBeat(int currentBeat)
        {
            
        }

        protected void FinishEarly()
        {
            MicrogameTimer.Stop();
            OnTimerFinished();
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        protected void PlaySuccessSfx()
        {
            _SuccessSfx.Play();
        }

        protected void PlayFailureSfx()
        {
            _FailureSfx.Play();
        }

        // Subclass functions
        protected void ShowTimer(float duration = 0.25f)
        {
            EmitSignal(nameof(ShowTimerRequested), duration);
        }

        protected void HideTimer(float duration = 0.25f)
        {
            EmitSignal(nameof(HideTimerRequested), duration);
        }

        protected void ShowBackground(float duration = 0.25f)
        {
            EmitSignal(nameof(ShowBackgroundRequested), duration);
        }

        protected void HideBackground(float duration = 0.25f)
        {
            EmitSignal(nameof(HideBackgroundRequested), duration);
        }

        protected Vector2 GetDirInput()
        {
            return new Vector2(
                Mathf.Sign(
                    Input.GetAxis("move_left", "move_right") +
                    Input.GetAxis("fps_move_left", "fps_move_right") +
                    Input.GetAxis("fps_look_left", "fps_look_right")
                ),
                Mathf.Sign(
                    Input.GetAxis("move_down", "move_up") +
                    Input.GetAxis("fps_move_backward", "fps_move_forward") +
                    Input.GetAxis("fps_look_down", "fps_look_up")
                )
            );
        }

        // Internal functions
        private void OnBeatTimerTick()
        {
            // Increment current beat
            CurrentBeat += 1;

            // Tick the beat hook
            OnBeat(CurrentBeat);
            EmitSignal(nameof(BeatTicked), CurrentBeat);

            // If current beat is max beats, finish
            if (CurrentBeat == NumBeats)
            {
                MicrogameTimer.Stop();
                OnTimerFinished();
            }
        }
    }
}