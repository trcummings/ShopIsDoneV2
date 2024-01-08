using Godot;
using System;

namespace ShopIsDone.Tasks.TaskModels
{
    // This is a render for task interactables to show task progress
    public interface ITaskInteractableRenderModel
    {
        void SetProgress(int amount, int current, int total);
    }

    public partial class TaskModelHelper : Node
    {
        public virtual void SetProgress(int amount, int current, int total)
        {
            // Do nothing
        }
    }
}

