using Godot;
using System;

namespace ShopIsDone.Microgames.HallwayChase
{
    public partial class HallwayChaseMicrogame : Microgame
    {
        private HallwayChase _HallwayChase;

        public override void _Ready()
        {
            base._Ready();
            _HallwayChase = GetNode<HallwayChase>("%HallwayChase");
        }

        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);

            // Connect to the hallway chase finish state
            _HallwayChase.Connect(nameof(_HallwayChase.PlayerReachedGoal), new Callable(this, nameof(OnPlayerReachedGoal)));
            _HallwayChase.Connect(nameof(_HallwayChase.PlayerFailed), new Callable(this, nameof(OnPlayerFailed)));

            // Init the hallway chase
            _HallwayChase.Init();
        }

        public override void Start()
        {
            // NB: Do NOT call base.Start() here. That will run too many
            // functions we want to call later

            // Hide the timer bar
            HideTimer(1.5f);
            // Show the background... slowly
            ShowBackground(2.5f);
        
            // Start the hallway chase game
            _HallwayChase.Start();
        }

        private async void OnPlayerReachedGoal()
        {
            // Set outcome to win
            Outcome = Outcomes.Win;

            // Play success sound
            PlaySuccessSfx();

            // Wait a tick
            await ToSignal(GetTree().CreateTimer(1f), "timeout");

            // Finish
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private async void OnPlayerFailed()
        {
            // Set outcome to win
            Outcome = Outcomes.Loss;

            // Play success sound
            PlayFailureSfx();

            // Wait a tick
            await ToSignal(GetTree().CreateTimer(1f), "timeout");

            // Finish
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }
    }
}
