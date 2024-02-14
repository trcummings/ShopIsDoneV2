using Godot;
using Godot.Collections;
using ShopIsDone.ArenaInteractions;
using ShopIsDone.Tasks;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class TaskState : ActionState
    {
        [Export]
        private UnitTaskService _UnitTaskService;

        private bool _JustConfirmedTask = false;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Set flag
            _JustConfirmedTask = false;

            // Wire up unit interaction service
            _UnitTaskService.ConfirmedInteraction += OnConfirmTask;
            _UnitTaskService.CanceledInteraction += OnCancel;
            _UnitTaskService.Activate(_SelectedUnit);
        }

        public override void OnExit(string nextState)
        {
            // If next state is "Idle" then we probably got interrupted, reset
            // the service
            if (!_JustConfirmedTask && nextState == Consts.States.IDLE)
            {
                _UnitTaskService.Reset();
            }

            // Reset flag
            _JustConfirmedTask = false;

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
            // Set flag
            _JustConfirmedTask = true;

            // Emit
            EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
            {
                { Consts.ACTION_KEY, _Action },
                { Consts.TASK_KEY, task }
            });
        }
    }
}