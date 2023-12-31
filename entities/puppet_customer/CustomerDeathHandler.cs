using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Arenas;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class CustomerDeathHandler : BasicDeathHandler
    {
        [Export]
        private CharacterBody3D _Body;

        [Inject]
        private UnitDeathService _DeathService;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        public override Command Die()
        {
            return new SeriesCommand(
                base.Die(),
                // Move to far off point and disable
                new ActionCommand(() => _DeathService.SafelyRemoveUnit(Entity))
            );
        }
    }

}
