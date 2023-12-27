using System;
using System.Threading.Tasks;

namespace ShopIsDone.Utils.Commands
{
    // Command for running async callback. NB: USE SPARINGLY
    public partial class AsyncCommand : Command
    {
        protected Func<Task> CommandAction;

        public AsyncCommand(Func<Task> commandAction)
        {
            CommandAction = commandAction;
        }

        public async override void Execute()
        {
            await CommandAction();
            Finish();
        }
    }
}
