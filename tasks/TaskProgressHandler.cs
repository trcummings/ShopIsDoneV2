using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Tasks
{
    public interface ITaskProgressHandler
    {
        bool CanProgressTask(TaskComponent task);

        Command OnProgressTask(TaskComponent task);

        Command OnFailToProgress(TaskComponent task);
    }

    public partial class TaskProgressHandler : Node, ITaskProgressHandler
    {
        [Signal]
        public delegate void TaskProgressFailedEventHandler();

        // Default to true so nothing stops us
        public virtual bool CanProgressTask(TaskComponent task)
        {
            return true;
        }

        public virtual Command OnFailToProgress(TaskComponent task)
        {
            return new Command();
        }

        public virtual Command OnProgressTask(TaskComponent task)
        {
            return new Command();
        }
    }
}