using System;
using System.Linq;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.AI
{
    public partial class ActionPlanner : NodeComponent
    {

        [Export]
        protected Node3D _SensorsNode;
        protected Array<Sensor> _Sensors;

        [Export]
        protected ActionHandler _ActionHandler;

        private Array<TurnPlan> _TurnPlans = new Array<TurnPlan>();

        // The blackboard is the "mind" of the AI, where values get recorded for
        // sensors to update, and for turns and action plans to use
        protected Dictionary<string, Variant> _Blackboard = new Dictionary<string, Variant>();

        public override void _Ready()
        {
            base._Ready();
            _Sensors = _SensorsNode.GetChildren().OfType<Sensor>().ToGodotArray();
        }

        public override void Init()
        {
            base.Init();

            // Pull plans from children of planner
            _TurnPlans = GetChildren().OfType<TurnPlan>().ToGodotArray();

            // Init turn plans
            foreach (var turnPlan in _TurnPlans)
            {
                turnPlan.Init(_Blackboard, _ActionHandler.Actions);
            }
            // Init sensors
            foreach (var sensor in _Sensors) sensor.Init();
        }

        public void ResetPlanner()
        {
            // Reset all turn plans
            foreach (var turnPlan in _TurnPlans) turnPlan.ResetTurnPlan();
        }

        public void Think()
        {
            // Ignore if entity is not active
            if (!Entity.IsInArena() || !Entity.IsActive()) return;
            // Update sensors
            foreach (var sensor in _Sensors) sensor.Sense(Entity, _Blackboard);
        }

        public Command Act()
        {
            // Sort remaining actions by priority
            var highestPriorityPlan = _TurnPlans
                .Where(a => a.IsValid())
                .OrderByDescending(a => a.GetPriority())
                .First();

            // Execute next step of the turn plan
            return GetCurrentPlan().ExecuteNextAction();
        }

        public bool CanStillAct()
        {
            // Action handler check
            if (!_ActionHandler.HasAvailableActions()) return false;

            // We can act if we have a valid turn plan
            return _TurnPlans.Any(tp => tp.IsValid());
        }

        private TurnPlan GetCurrentPlan()
        {
            // Sort plans by priority
            return _TurnPlans
                .Where(a => a.IsValid())
                .OrderByDescending(a => a.GetPriority())
                .First();
        }
    }
}
