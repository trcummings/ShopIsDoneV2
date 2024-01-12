using Godot;
using ShopIsDone.Conditions;
using ShopIsDone.Core;
using ShopIsDone.Tasks;

namespace ShopIsDone.Levels.TestLevel.Conditions
{
    public partial class TaskCompletedCondition : Condition
    {
        [Export]
        private LevelEntity Task;

        public override bool IsComplete()
        {
            return Task.GetComponent<TaskComponent>().IsTaskComplete();
        }
    }
}