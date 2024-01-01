using Godot;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Microgames
{
    // This is a component for an entity to use to initiate microgames
    public partial class MicrogameHandler : NodeComponent
    {
        [Export]
        public PackedScene MicrogameScene;

        [Export]
        private NodePath _OutcomeHandlerPath;
        private IOutcomeHandler _OutcomeHandler;

        [Inject]
        private MicrogameController _MicrogameController;

        private Microgame _MicrogameScene;

        public override void _Ready()
        {
            base._Ready();
            _OutcomeHandler = GetNode<IOutcomeHandler>(_OutcomeHandlerPath);
        }

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        public string GetMicrogamePrompt(MicrogamePayload payload)
        {
            return GetMicrogame(payload).PromptText;
        }

        private Microgame GetMicrogame(MicrogamePayload _)
        {
            _MicrogameScene = MicrogameScene.Instantiate<Microgame>();
            return _MicrogameScene;
        }

        public Command RunMicrogame(MicrogamePayload payload)
        {
            // Set microgame on payload
            if (payload.Microgame == null) payload.Microgame = GetMicrogame(payload);
            // If no handler given on payload, use the default handler
            if (payload.Source == null) payload.Source = _OutcomeHandler;

            // Run the microgame
            return _MicrogameController.RunMicrogame(payload);
        }
    }
}
