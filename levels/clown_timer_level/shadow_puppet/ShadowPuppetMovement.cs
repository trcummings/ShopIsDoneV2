using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using System;
using System.Threading.Tasks;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Arenas.ClownTimer;
using ShopIsDone.Arenas;

namespace ShopIsDone.Levels.ClownTimerLevel.ShadowPuppets
{
    public partial class ShadowPuppetMovement : NodeComponent, IClownTimerTick
    {
        [Export]
        private Label3D _Label;

        [Inject]
        private TurnInterrupterService _TurnInterrupter;

        [Inject]
        private PlayerUnitService _PlayerUnitService;

        private LevelEntity _Target;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);

            // Get target
            _Target = _PlayerUnitService.GetActiveUnits().Find(e => e.Id == "haskell");
        }

        public void StartClownTimer()
        {

        }

        public void StopClownTimer()
        {

        }

        public void ClownTimerTick(double delta)
        {
            if (_Target == null) return;

            _Label.Text = Entity.TilemapPosition.ToString();

            // Move towards target
            Entity.Velocity = Entity.GlobalPosition.DirectionTo(_Target.GlobalPosition) * 10 * (float)delta;
            Entity.MoveAndSlide();
        }
    }
}

