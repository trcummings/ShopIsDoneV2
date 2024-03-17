using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.UI;
using System;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class CustomerInfoUI : Control, ITargetUI
    {
        // Nodes
        private DiffableProgressBar _HealthBar;
        private Label _Damage;
        private LevelEntity _Customer;

        public override void _Ready()
        {
            // Ready nodes
            _HealthBar = GetNode<DiffableProgressBar>("%HealthBar");
            _Damage = GetNode<Label>("%Damage");
        }

        public bool IsValidEntityForUI(LevelEntity entity)
        {
            return entity.HasComponent<CustomerComponent>();
        }

        public void Init(LevelEntity customer)
        {
            _Customer = customer;

            var apHandler = customer.GetComponent<ActionPointHandler>();
            var outcomeHandler = customer.GetComponent<IOutcomeHandler>();

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
            // Show movement range and damage range
        }

        public void CleanUp()
        {
            // Clear away tile indicators
        }
    }
}

