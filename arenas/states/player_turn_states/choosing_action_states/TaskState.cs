using Godot;
using Godot.Collections;
using ShopIsDone.Tasks;
using ShopIsDone.Tasks.UI;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class TaskState : ActionState
    {
        [Export]
        private UnitTaskService _UnitTaskService;

        [Export]
        private TaskInfoUI _TaskInfoUI;

        private bool _JustConfirmedTask = false;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Set flag
            _JustConfirmedTask = false;

            // Wire up unit task service
            _UnitTaskService.ConfirmedTask += OnConfirmTask;
            _UnitTaskService.Canceled += OnCancel;
            _UnitTaskService.SelectedTask += OnTaskSelected;
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

            // Clean up unit task service
            _UnitTaskService.ConfirmedTask -= OnConfirmTask;
            _UnitTaskService.Canceled -= OnCancel;
            _UnitTaskService.Deactivate();

            // Hide task UI
            _TaskInfoUI.Hide();
            _TaskInfoUI.ClearDiff();

            // Base OnExit
            base.OnExit(nextState);
        }

        private void OnTaskSelected(TaskComponent task)
        {
            _TaskInfoUI.Init(task.Entity);
            _TaskInfoUI.Show();
            _TaskInfoUI.SetDiff(1);
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