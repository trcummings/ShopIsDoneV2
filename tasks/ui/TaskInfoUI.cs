using System.Linq;
using Godot;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using ShopIsDone.Models;
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
        private ModelComponent _Model;

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
            return entity.HasComponent<TaskComponent>() && entity.IsActive();
        }

        public override void Init(LevelEntity entity)
        {
            base.Init(entity);
            InjectionProvider.Inject(this);

            // Get components
            _Task = entity.GetComponent<TaskComponent>();
            _Model = entity.GetComponent<ModelComponent>();

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

        protected override MoreInfoPayload GetInfoPayload()
        {
            return new MoreInfoPayload()
            {
                Title = _Entity.EntityName,
                Description = _Task.TaskDescription,
                Model = (IModel)(_Model.Model as Node3D).Duplicate(),
                Point = _Entity.WidgetPoint.Position with { Y = 0 }
            };
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
            base.CleanUp();
            // Clear away tile indicators
            _TileIndicator.ClearIndicators();
        }
    }
}

