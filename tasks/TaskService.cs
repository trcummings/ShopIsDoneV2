using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShopIsDone.Cameras;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Tasks
{
    public partial class TaskService : Node, IService
    {
        [Inject]
        private CameraService _CameraService;

        public void Init()
        {
            InjectionProvider.Inject(this);
        }

        public Command ResolveInProgressTasks()
        {
            // Resolve or progress all currently ongoing tasks
            return new SeriesCommand(
                GetInProgressTasks()
                    .Select(ResolveInProgressTask)
                    .ToArray()
            );
        }

        // Private
        private List<TaskComponent> GetInProgressTasks()
        {
            return GetTree()
                .GetNodesInGroup("entities")
                .OfType<LevelEntity>()
                .Where(e => e.HasComponent<TaskComponent>())
                .Select(e => e.GetComponent<TaskComponent>())
                // Get tasks where they are incomplete, but they have operators waiting
                .Where(task => !task.IsTaskComplete() && task.Operators.Count > 0)
                .ToList();
        }

        private Command ResolveInProgressTask(TaskComponent task)
        {
            return new SeriesCommand(
                // Center the camera on the first operator
                new DeferredCommand(() =>
                {
                    Node3D firstOperator = task.Operators[0]?.Entity;

                    return new ConditionalCommand(
                        () => firstOperator != null,
                        _CameraService.WaitForCameraToReachTarget(firstOperator)
                    );
                }),
                // Zoom camera
                _CameraService.ZoomCameraTo(0.5f),
                // Ready operators
                new DeferredCommand(task.ReadyOperators),
                // Progress the task
                new DeferredCommand(task.ProgressTask),
                // If the task has progressed, conditionally complete it
                new ConditionalCommand(
                    task.IsTaskComplete,
                    task.CompleteTask()
                ),
                // Unzoom camera
                _CameraService.ZoomCameraTo(1f)
            );
        }
    }
}
