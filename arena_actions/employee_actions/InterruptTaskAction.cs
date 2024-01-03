using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Tasks;
using ShopIsDone.Utils.Commands;
using System;

namespace ShopIsDone.Actions
{
	public partial class InterruptTaskAction : ArenaAction
	{
        private TaskHandler _TaskHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _TaskHandler = Entity.GetComponent<TaskHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<TaskHandler>();
        }

        // Visible in menu if we're in the doing_task state, as most actions are
        public override bool IsVisibleInMenu()
        {
            return _TaskHandler.HasCurrentTask();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return new SeriesCommand(
                // Add the base execution command (mark the action as completed)
                base.Execute(message),
                // Stop doing the current task
                _TaskHandler.StopDoingTask()
            );
        }
    }
}
