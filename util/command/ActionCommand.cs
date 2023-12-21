using System;

namespace ShopIsDone.Utils.Commands
{
    public partial class ActionCommand : Command
    {
        protected Action CommandAction;

        public ActionCommand(Action commandAction)
        {
            CommandAction = commandAction;
        }

        public override void Execute()
        {
            CommandAction();
            Finish();
        }
    }
}

