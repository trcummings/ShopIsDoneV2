using System.Linq;
using Godot;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using ShopIsDone.UI;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;

namespace ShopIsDone.Tasks.UI
{
    public partial class TaskInfoUI : TargetInfoUI
    {
        // Nodes
        private DiffableProgressBar _HealthBar;
        private Label _Damage;
        private Label _APCost;
        private Label _AllowedNumber;
        private Label _RequiredEmployees;

        [Inject]
        protected TileIndicator _TileIndicator;

        private TaskComponent _Task;

        public override void _Ready()
        {
            // Ready nodes
            _HealthBar = GetNode<DiffableProgressBar>("%HealthBar");
            _Damage = GetNode<Label>("%Damage");
            _APCost = GetNode<Label>("%APCost");
            _AllowedNumber = GetNode<Label>("%AllowedNumber");
            _RequiredEmployees = GetNode<Label>("%RequiredEmployees");
        }

        public override bool IsValidEntityForUI(LevelEntity entity)
        {
            return entity.HasComponent<TaskComponent>();
        }

        public override void Init(LevelEntity entity)
        {
            base.Init(entity);
            InjectionProvider.Inject(this);

            // Get task component
            _Task = entity.GetComponent<TaskComponent>();

            // Get current number of operators
            var numOperators = _Task.Operators.Count;

            // Set progress bar
            _HealthBar.MaxValue = _Task.MaxTaskHealth;
            _HealthBar.Value = _Task.TaskHealth;
            _HealthBar.ShowDiff = false;

            // Set AP Cost
            _Damage.Text = _Task.TaskDamage.ToString();
            _APCost.Text = _Task.APCostPerTurn.ToString();
            _AllowedNumber.Text = $"{numOperators} / {_Task.OperatorsAllowed}";
            _RequiredEmployees.Text = $"{numOperators} / {_Task.OperatorsRequired}";
        }

        public override void SetDiff(int amount)
        {
            _HealthBar.DiffValue = Mathf.Max(_HealthBar.Value - amount, 0);
            _HealthBar.ShowDiff = true;
        }

        public override void ClearDiff()
        {
            _HealthBar.ShowDiff = false;
        }

        public override void ShowTileInfo()
        {
            // Show tile indicators for squares units can start the task on
            _TileIndicator.CreateIndicators(
                _Task
                    .GetSelectTiles()
                    .Where(t => t.IsLit())
                    .Select(t => t.GlobalPosition),
                TileIndicator.IndicatorColor.Yellow
            );
        }

        public override void CleanUp()
        {
            // Clear away tile indicators
            _TileIndicator.ClearIndicators();
        }
    }
}

