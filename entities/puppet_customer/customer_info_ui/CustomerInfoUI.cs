using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Models;
using ShopIsDone.Tiles;
using ShopIsDone.UI;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Pathfinding;
using ShopIsDone.Widgets;
using System;
using System.Linq;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class CustomerInfoUI : TargetInfoUI
    {
        // Nodes
        private DiffableProgressBar _HealthBar;
        private Label _Damage;

        [Inject]
        protected TileIndicator _TileIndicator;

        [Inject]
        private TileManager _TileManager;

        private ModelComponent _Model;
        private CustomerComponent _Customer;

        public override void _Ready()
        {
            // Ready nodes
            _HealthBar = GetNode<DiffableProgressBar>("%HealthBar");
            _Damage = GetNode<Label>("%Damage");
        }

        public override bool IsValidEntityForUI(LevelEntity entity)
        {
            return entity.HasComponent<CustomerComponent>() && entity.IsActive();
        }

        public override void Init(LevelEntity entity)
        {
            base.Init(entity);
            InjectionProvider.Inject(this);

            _Model = entity.GetComponent<ModelComponent>();
            _Customer = entity.GetComponent<CustomerComponent>();

            var apHandler = _Entity.GetComponent<ActionPointHandler>();
            var outcomeHandler = _Entity.GetComponent<IOutcomeHandler>();

            // Set progress bar
            _HealthBar.MaxValue = apHandler.MaxActionPoints;
            _HealthBar.Value = apHandler.ActionPoints;
            _HealthBar.ShowDiff = false;

            // Set Damage
            _Damage.Text = outcomeHandler.GetDamage(
                new Microgames.MicrogamePayload() {
                    Outcome = Microgames.Microgame.Outcomes.Win
                }
            ).Damage.ToString();
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

        protected override MoreInfoPayload GetInfoPayload()
        {
            return new MoreInfoPayload()
            {
                Title = _Entity.EntityName,
                Description = _Customer.Description,
                Model = (IModel)(_Model.Model as Node3D).Duplicate(),
                Point = _Entity.WidgetPoint.Position with { Y = 0 }
            };
        }

        public override void ShowTileInfo()
        {
            // Get tile
            var tile = _TileManager.GetTileAtTilemapPos(_Entity.TilemapPosition);

            // Get move range
            var moveRange = _Entity.GetComponent<TileMovementHandler>().BaseMove;

            // Get max range of actions
            var maxActionRange = _Entity
                .GetComponent<ActionHandler>()
                .Actions
                // Action is hostile
                .Where(a => a.ActionType == ArenaAction.ActionTypes.Target)
                .Where(a => a.TargetDisposition == ArenaAction.DispositionTypes.Enemies)
                .OrderBy(a => a.Range)
                .Select(a => a.Range)
                .FirstOrDefault();


            // Show movement range and damage range
            var availableTiles = new MoveGenerator()
                // Combine ranges
                .GetAvailableMoves(tile, true, moveRange + maxActionRange)
                // Filter tiles out with obstacles on them or unlit tiles
                .Where(tile => !tile.HasObstacleOnTile)
                .Where(tile => tile.IsLit())
                .ToList();

            // Show indicators
            _TileIndicator.CreateIndicators(
                availableTiles.Select(t => t.GlobalPosition),
                TileIndicator.IndicatorColor.Red
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

