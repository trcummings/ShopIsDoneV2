using Godot;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Audio;
using ShopIsDone.Core;
using ShopIsDone.Models;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Entities.PuppetCustomers.Actions
{
    public partial class IntimidateAction : ArenaAction
    {
        [Export]
        private AudioStream _IntimidateSfx;

        private ModelComponent _Model;

        [Inject]
        private SfxService _SfxService;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _Model = Entity.GetComponent<ModelComponent>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<ModelComponent>();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return new SeriesCommand(
                base.Execute(message),
                // Intimidate animation and SFX
                new ParallelCommand(
                    _Model.RunPerformAction(StateConsts.Customers.INTIMIDATE),
                    _SfxService.RunPlaySfx(_IntimidateSfx)
                )
            );
        }
    }
}