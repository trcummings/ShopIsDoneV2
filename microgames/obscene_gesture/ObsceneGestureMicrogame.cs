namespace ShopIsDone.Microgames.ObsceneGesture
{
    public partial class ObsceneGestureMicrogame : Microgame
	{
        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);
            HideTimer(0.001f);
        }
        public override void Start()
        {
            // Give this thing half the number of beats
            NumBeats = 5;

            // Base call
            base.Start();
        }

        protected override void OnTimerFinished()
        {
            // After timer is finished, just emit an outcome
            EmitSignal(nameof(MicrogameFinished), (int)Outcomes.Loss);
        }
    }
}

