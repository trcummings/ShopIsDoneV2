using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Tasks;
using ShopIsDone.Utils.Commands;
using ShopIsDone.EntityStates;
using System;
using ShopIsDone.Actions;
using ActionConsts = ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Consts;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Employees.Actions
{
    public partial class StartTaskAction : ArenaAction
    {
        private TaskHandler _TaskHandler;
        private EntityStateHandler _StateHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _TaskHandler = Entity.GetComponent<TaskHandler>();
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<TaskHandler>() &&
                entity.HasComponent<EntityStateHandler>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<TaskComponent>();
        }

        // Visible if we don't have a current task, and we're in range of a task
        public override bool IsVisibleInMenu()
        {
            return !_TaskHandler.HasCurrentTask();
        }

        public override bool IsAvailable()
        {
            // Base available check
            if (!base.IsAvailable()) return false;

            // Otherwise, it's only available if we're in range of a task and
            // we're idling
            return
                _TaskHandler.HasAvailableTasksInRange() &&
                _StateHandler.IsInState(StateConsts.IDLE);
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get target task component
            var task = (TaskComponent)message[ActionConsts.TASK_KEY];

            return new SeriesCommand(
                // Add the base execution command (mark the action as completed)
                base.Execute(message),
                // Start doing the current task
                _TaskHandler.StartTask(task)
            );
        }
    }
}
