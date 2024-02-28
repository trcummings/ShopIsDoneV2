using Godot;

namespace ShopIsDone.Tasks.UI
{
    public partial class TaskInfoUI : Control
    {
        // Nodes
        private ProgressBar _HealthBar;
        private Label _APCost;
        private Label _AllowedNumber;
        private Label _RequiredEmployees;

        public override void _Ready()
        {
            // Ready nodes
            _HealthBar = GetNode<ProgressBar>("%HealthBar");
            _APCost = GetNode<Label>("%APCost");
            _AllowedNumber = GetNode<Label>("%AllowedNumber");
            _RequiredEmployees = GetNode<Label>("%RequiredEmployees");
        }

        public void Init(TaskComponent task)
        {
            // Get current number of operators
            var numOperators = task.Operators.Count;

            // Set progress bar
            _HealthBar.MinValue = 0;
            _HealthBar.MaxValue = task.MaxTaskHealth;
            _HealthBar.Value = task.TaskHealth;

            // Set AP Cost
            _APCost.Text = task.APCostPerTurn.ToString();
            _AllowedNumber.Text = $"{numOperators} / {task.OperatorsAllowed}";
            _RequiredEmployees.Text = $"{numOperators} / {task.OperatorsRequired}";
        }
    }
}

