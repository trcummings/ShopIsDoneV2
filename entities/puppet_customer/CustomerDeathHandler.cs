using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Arenas;
using ShopIsDone.Tasks;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class CustomerDeathHandler : BasicDeathHandler
    {
        [Export]
        private CharacterBody3D _Body;

        [Export]
        public int ExcessApGranted = 1;

        [Inject]
        private UnitDeathService _DeathService;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        public override Command Die(ApDamagePayload damagePayload)
        {
            var source = damagePayload.Source;
            return new SeriesCommand(
                base.Die(damagePayload),
                // Move to far off point and disable
                new ActionCommand(() => {
                    // Disable body collder
                    Entity.GetNode<CollisionShape3D>("CollisionShape").Disabled = true;
                    // Remove unit
                    _DeathService.SafelyRemoveUnit(Entity);
                }),
                // Reward unit that killed it
                new ConditionalCommand(
                    source.HasComponent<TaskHandler>,
                    new ActionCommand(() =>
                    {
                        var handler = source.GetComponent<TaskHandler>();
                        handler.RewardTaskCompletion(ExcessApGranted);
                    })
                )
            );
        }
    }

}
