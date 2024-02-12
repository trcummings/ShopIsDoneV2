using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Tasks
{
	public partial class TaskCompletionHandler : Node
	{
        public virtual Command OnTaskCompleted(TaskComponent task)
        {
            return new Command();
        }
    }
}

