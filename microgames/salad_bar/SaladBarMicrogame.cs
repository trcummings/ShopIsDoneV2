using Godot;
using System;
using System.Linq;
using ShopIsDone.Utils;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class SaladBarMicrogame : Microgame
    {
        private SaladBarArena _SaladBarArena;
        private HotbarQueue _HotbarQueue;
        private HealthBar _HealthBar;
        private AudioStreamPlayer _RoomTone;
        private WaveManager _WaveManager;

        [Export]
        private Camera2D _Camera;

        public override void _Ready()
        {
            base._Ready();

            _SaladBarArena = GetNode<SaladBarArena>("%SaladBarArena");
            _HotbarQueue = GetNode<HotbarQueue>("%HotbarQueue");
            _HealthBar = GetNode<HealthBar>("%HealthBar");
            _RoomTone = GetNode<AudioStreamPlayer>("%RoomTone");
            _WaveManager = GetNode<WaveManager>("%WaveManager");

            SetProcess(false);
            SetPhysicsProcess(false);

            // Fade in the room tone
            var goalDb = _RoomTone.VolumeDb;
            _RoomTone.VolumeDb = -80;
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_RoomTone, "volume_db", goalDb, 2f)
                .SetEase(Tween.EaseType.Out);
            _RoomTone.Play();

            // Connect to wave manager
            _WaveManager.AllWavesFinished += OnAllWavesFinished;
        }

        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);

            // Hide the timer bar
            HideTimer(0.01f);

            // Initialize health bar
            // Make health dependent on number of participants in microgame
            var numTargets = payload.Targets?.Length ?? 1;
            // Max out at 3
            _HealthBar.Init(Mathf.Min(numTargets, 3));
        }

        public override void Start()
        {
            // Allow input processing
            SetProcess(true);
            SetPhysicsProcess(true);

            // Start wave manager
            _WaveManager.StartNextWave();
        }

        public override void _PhysicsProcess(double _)
        {
            _SaladBarArena.MoveInput = GetDirInput();
        }

        public async void OnDied()
        {
            // Stop game
            Stop();

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();

            // Wait a moment
            await ToSignal(GetTree().CreateTimer(1f), "timeout");

            // Finish early
            FinishEarly();
        }

        public void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 1000;
        }

        private void OnAllWavesFinished()
        {
            // Stop game
            Stop();

            // Set outcome to win
            Outcome = Outcomes.Win;

            // Play success sound
            PlaySuccessSfx();

            // Finish early
            FinishEarly();
        }

        private void Stop()
        {
            var stoppables = GetTree().GetNodesInGroup("salad_bar_stoppable").OfType<IStoppable>();
            foreach (var stoppable in stoppables) stoppable.Stop();
            _WaveManager.Stop();
            _SaladBarArena.StopFlyTimer();
            _SaladBarArena.SetProcess(false);
            _SaladBarArena.SetPhysicsProcess(false);
            SetProcess(false);
            SetPhysicsProcess(false);
        }
    }
}

