using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Tasks
{
	public partial class TaskCompletionHandler : Node
	{
        public Command OnTaskCompleted(TaskComponent _)
        {
            return new Command();
        }
    }
}

