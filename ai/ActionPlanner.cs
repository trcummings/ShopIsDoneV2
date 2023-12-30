using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShopIsDone.Actions;
using Lighting;
using ShopIsDone.Core;
using ShopIsDone.Arenas;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.AI
{
    public partial class ActionPlanner : NodeComponent
    {
        [Export]
        public TurnPlan TurnPlan = new TurnPlan();

        // Properties
        protected Dictionary<string, Variant> _Blackboard = new Dictionary<string, Variant>();
        protected List<ActionPlan> _ActionPlans;

        // Nodes
        [Export]
        protected Node3D _Sensors;

        [Export]
        protected ActionHandler _ActionHandler;

        public override void Init()
        {
            base.Init();
            // TODO: Initialize action plans and sensors
        }

        public Command Think()
        {
            return new ConditionalCommand(
                // Return early if we're not active
                () => Entity.IsInArena() && Entity.IsActive(),
                // Check sensors to update agent state and blackboard
                new SeriesCommand(
                    GetSensors()
                        .Select(sensor => new ActionCommand(sensor.Sense))
                        .ToArray()
                )
            );
        }

        public Command Act()
        {
            // Sort remaining actions by priority
            var highestPriorityPlan = _ActionPlans
                .Where(a => a.IsValid())
                .OrderByDescending(a => a.GetPriority())
                .First();

            // Perform action
            return highestPriorityPlan.ExecuteAction();
        }

        public bool CanStillAct()
        {
            // Action handler check
            if (!_ActionHandler.HasAvailableActions()) return false;

            // If any of our plans are valid, we can still act
            var hasAnyValidPlans = _ActionPlans.Any(planner => planner.IsValid());
            return hasAnyValidPlans;
        }

        private IEnumerable<Sensor> GetSensors()
        {
            if (_Sensors == null) return new List<Sensor>();
            return _Sensors.GetChildren().OfType<Sensor>();
        }
    }
}
