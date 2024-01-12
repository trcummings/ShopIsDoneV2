using Godot;
using Godot.Collections;
using ShopIsDone.Tasks;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class TaskState : ActionState
    {
        [Export]
        private UnitTaskService _UnitTaskService;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Wire up unit interaction service
            _UnitTaskService.ConfirmedInteraction += OnConfirmTask;
            _UnitTaskService.CanceledInteraction += OnCancel;
            _UnitTaskService.Activate(_SelectedUnit);
        }

        public override void OnExit(string nextState)
        {
            // Clean up unit interaction service
            _UnitTaskService.ConfirmedInteraction -= OnConfirmTask;
            _UnitTaskService.CanceledInteraction -= OnCancel;
            _UnitTaskService.Deactivate();

            // Base OnExit
            base.OnExit(nextState);
        }

        private void OnCancel()
        {
            EmitSignal(nameof(MainMenuRequested));
        }

        private void OnConfirmTask(TaskComponent task)
        {
            EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
            {
                { Consts.ACTION_KEY, _Action },
                { Consts.TASK_KEY, task }
            });
        }
    }
}