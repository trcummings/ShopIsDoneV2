using Godot;
using ShopIsDone.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShopIsDone.Arenas.UI
{
    public interface ITargetUI
    {
        void Init(LevelEntity entity);

        void ShowTileInfo();

        void Show();

        void Hide();

        void SetDiff(int amount);

        void ClearDiff();

        void CleanUp();

        bool IsValidEntityForUI(LevelEntity entity);

        void RequestInfoPayload(Action<MoreInfoPayload> payloadCb);
    }

    // This is a manager that delegates selection of a selected/targeted
    // entity's UI
    public partial class InfoUIContainer : VBoxContainer
    {
        [Signal]
        public delegate void InfoPanelRequestedEventHandler(MoreInfoPayload payload);

        public ITargetUI CurrentUI { get { return _CurrentUI; } }
        private ITargetUI _CurrentUI;
        private LevelEntity _CurrentEntity = null;
        private List<ITargetUI> _TargetUIList = new List<ITargetUI>();

        public override void _Ready()
        {
            base._Ready();

            // Get all children that are targeting UI
            _TargetUIList = GetChildren()
                .OfType<ITargetUI>()
                .ToList();
        }

        public bool HasCurrentUI()
        {
            return CurrentUI != null;
        }

        public bool HasTargetableUIForEntity(LevelEntity entity)
        {
            return _TargetUIList.Any(t => t.IsValidEntityForUI(entity));
        }

        public void SetDiff(int amount)
        {
            _CurrentUI?.SetDiff(amount);
        }

        public void Init(LevelEntity entity)
        {
            if (_CurrentEntity != entity)
            {
                // Clean up first
                CleanUp();

                // Set current entity
                _CurrentEntity = entity;

                // Find the correct target UI for the entity if one exists
                _CurrentUI = _TargetUIList.Find(t => t.IsValidEntityForUI(entity));
                if (_CurrentUI != null)
                {
                    _CurrentUI.Init(entity);
                    _CurrentUI.Show();
                }
            }
        }

        public void RequestInfoPanel()
        {
            _CurrentUI?.RequestInfoPayload((MoreInfoPayload payload) =>
            {
                EmitSignal(nameof(InfoPanelRequested), payload);
            });
        }

        public void ShowTileInfo()
        {
            _CurrentUI?.ShowTileInfo();
        }

        public void CleanUp()
        {
            // Clear and reset the current UI if we have one
            if (_CurrentUI != null)
            {
                _CurrentUI.ClearDiff();
                _CurrentUI.CleanUp();
                _CurrentUI.Hide();
                _CurrentUI = null;
                _CurrentEntity = null;
            }
        }
    }
}

