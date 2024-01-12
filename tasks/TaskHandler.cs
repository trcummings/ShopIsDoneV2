using System;
using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.EntityStates;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using StateConsts = ShopIsDone.EntityStates.Consts;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Tasks
{
    // This is a component for an entity to use to interact with a task
    public partial class TaskHandler : NodeComponent
    {
        // Nodes
        [Export]
        private EntityStateHandler _StateHandler;

        [Export]
        private ActionPointHandler _ActionPointHandler;

        [Export]
        private Area3D _TaskDetector;

        public TaskComponent CurrentTask { get { return _CurrentTask; } }
        private TaskComponent _CurrentTask;

        public bool HasCurrentTask()
        {
            return _CurrentTask != null;
        }

        public Command StartTask(TaskComponent task)
        {
            return new SeriesCommand(
                // Change to DoingTask state
                _StateHandler.RunChangeState(StateConsts.Employees.DO_TASK),
                // Wait a bit
                new WaitForCommand(Entity, 0.25F),
                // Register the task to the pawn state and vice versa
                new ActionCommand(() =>
                {
                    _CurrentTask = task;
                    task.RegisterOperator(this);
                })
            );
        }

        public Command StopDoingTask()
        {
            return new SeriesCommand(
                // Deregister with task
                new ActionCommand(() =>
                {
                    _CurrentTask.DeregisterOperator(this);
                    _CurrentTask = null;
                }),
                // Go back to normal state
                _StateHandler.RunChangeState(StateConsts.IDLE),
                // Wait a moment for the result to have impact
                new WaitForCommand(Entity, 0.25f)
            );
        }

        public void RewardTaskCompletion(int excessAp)
        {
            _ActionPointHandler.GrantExcessAp(excessAp);
        }

        public bool CanProgressTask()
        {
            return _ActionPointHandler.HasEnoughAPForAction(_CurrentTask.APCostPerTurn, 0);
        }

        public void CommitAPToTask()
        {
            _ActionPointHandler.SpendAPOnAction(_CurrentTask.APCostPerTurn);
        }

        public bool HasAvailableTasksInRange()
        {
            return _TaskDetector
                .GetOverlappingAreas()
                .OfType<TaskSelectorTile>()
                .Select(tile => tile.Task)
                .Where(task => task.Entity.IsInArena())
                .Count() > 0;
        }

        // Interaction
        public Array<TaskComponent> GetTasksInRange()
        {
            return _TaskDetector
                .GetOverlappingAreas()
                .OfType<TaskSelectorTile>()
                .Select(tile => tile.Task)
                .Where(task => task.Entity.IsInArena())
                .ToGodotArray();
        }
    }
}
