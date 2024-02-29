using Godot;
using ShopIsDone.UI;

namespace ShopIsDone.Tasks.UI
{
    public partial class TaskInfoUI : Control
    {
        // Nodes
        private DiffableProgressBar _HealthBar;
        private Label _Damage;
        private Label _APCost;
        private Label _AllowedNumber;
        private Label _RequiredEmployees;

        public override void _Ready()
        {
            // Ready nodes
            _HealthBar = GetNode<DiffableProgressBar>("%HealthBar");
            _Damage = GetNode<Label>("%Damage");
            _APCost = GetNode<Label>("%APCost");
            _AllowedNumber = GetNode<Label>("%AllowedNumber");
            _RequiredEmployees = GetNode<Label>("%RequiredEmployees");
        }

        public void Init(TaskComponent task)
        {
            // Get current number of operators
            var numOperators = task.Operators.Count;

            // Set progress bar
            _HealthBar.MaxValue = task.MaxTaskHealth;
            _HealthBar.Value = task.TaskHealth;
            _HealthBar.ShowDiff = false;

            // Set AP Cost
            _Damage.Text = task.TaskDamage.ToString();
            _APCost.Text = task.APCostPerTurn.ToString();
            _AllowedNumber.Text = $"{numOperators} / {task.OperatorsAllowed}";
            _RequiredEmployees.Text = $"{numOperators} / {task.OperatorsRequired}";
        }

        public void SetHealthDiff(int amount)
        {
            _HealthBar.DiffValue = Mathf.Max(_HealthBar.Value - amount, 0);
            _HealthBar.ShowDiff = true;
        }

        public void ClearHealthDiff()
        {
            _HealthBar.ShowDiff = false;
        }
    }
}

