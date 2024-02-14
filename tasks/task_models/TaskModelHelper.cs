using Godot;
using System;

namespace ShopIsDone.Tasks.TaskModels
{
    // This is a render for task interactables to show task progress
    public interface ITaskInteractableRenderModel
    {
        void Init(int current, int total, float percent);

        void SetProgress(int amount, int current, int total, float percent);
    }

    public partial class TaskModelHelper : Node, ITaskInteractableRenderModel
    {
        public virtual void Init(int current, int total, float percent)
        {
            // Do nothing
        }

        public virtual void SetProgress(int amount, int current, int total, float percent)
        {
            // Do nothing
        }
    }
}

