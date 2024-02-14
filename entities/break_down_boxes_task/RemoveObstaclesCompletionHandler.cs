using Godot;
using ShopIsDone.Tasks;
using ShopIsDone.Utils.Commands;
using System;

namespace ShopIsDone.Entities.BreakDownBoxesTask
{
    public partial class RemoveObstaclesCompletionHandler : TaskCompletionHandler
    {
        public override Command OnTaskCompleted(TaskComponent task)
        {
            return new SeriesCommand(
                base.OnTaskCompleted(task),
                new ActionCommand(() =>
                {
                    // Disable all shape owner collisions
                    var ids = task.Entity.GetShapeOwners();
                    foreach (var id in ids)
                    {
                        task.Entity.ShapeOwnerSetDisabled((uint)id, true);
                    }
                })
            );
        }
    }
}

