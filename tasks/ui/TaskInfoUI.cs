using Godot;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using ShopIsDone.UI;

namespace ShopIsDone.Tasks.UI
{
    public partial class TaskInfoUI : Control, ITargetUI
    {
        // Nodes
        private DiffableProgressBar _HealthBar;
        private Label _Damage;
        private Label _APCost;
        private Label _AllowedNumber;
        private Label _RequiredEmployees;
        private LevelEntity _Task;

        public override void _Ready()
        {
            // Ready nodes
            _HealthBar = GetNode<DiffableProgressBar>("%HealthBar");
            _Damage = GetNode<Label>("%Damage");
            _APCost = GetNode<Label>("%APCost");
            _AllowedNumber = GetNode<Label>("%AllowedNumber");
            _RequiredEmployees = GetNode<Label>("%RequiredEmployees");
        }

        public bool IsValidEntityForUI(LevelEntity entity)
        {
            return entity.HasComponent<TaskComponent>();
        }

        public void Init(LevelEntity entity)
        {
            _Task = entity;

            // Get task component
            var task = entity.GetComponent<TaskComponent>();

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

        public void SetDiff(int amount)
        {
            _HealthBar.DiffValue = Mathf.Max(_HealthBar.Value - amount, 0);
            _HealthBar.ShowDiff = true;
        }

        public void ClearDiff()
        {
            _HealthBar.ShowDiff = false;
        }

        public void ShowTileInfo()
        {
            // Show tile indicators for squares units can start the task on
        }

        public void CleanUp()
        {
            // Clear away tile indicators
        }
    }
}

