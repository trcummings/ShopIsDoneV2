using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.AI
{
    public partial class ActionPlan : Resource
    {
        private ActionHandler _ActionHandler;

        protected ArenaAction _Action;

        public void Init(ActionHandler actionHandler, ArenaAction action)
        {
            _ActionHandler = actionHandler;
            _Action = action;
        }

        public virtual bool IsValid()
        {
            // Otherwise check if the action is available in the first place
            return _ActionHandler.IsActionAvailable(_Action);
        }

        public virtual int GetPriority()
        {
            return 1;
        }

        public virtual Command ExecuteAction()
        {
            // Do nothing
            return new Command();
        }
    }
}