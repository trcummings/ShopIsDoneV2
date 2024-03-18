using System;
using Godot;
using ShopIsDone.Core;

namespace ShopIsDone.Arenas.UI
{
	public partial class TargetInfoUI : Control, ITargetUI
    {
        protected LevelEntity _Entity;

        public virtual bool IsValidEntityForUI(LevelEntity entity)
        {
            return false;
        }

        public virtual void Init(LevelEntity entity)
        {
            _Entity = entity;
        }

        public virtual void RequestInfoPayload(Action<MoreInfoPayload> payloadCb)
        {
            payloadCb?.Invoke(GetInfoPayload());
        }

        protected virtual MoreInfoPayload GetInfoPayload()
        {
            // Override the payload here
            return new MoreInfoPayload();
        }

        public virtual void SetDiff(int amount)
        {
            // Override to show a damage diff
        }

        public virtual void ClearDiff()
        {
            // Override to clear a diff
        }

        public virtual void ShowTileInfo()
        {
            // Override to show movement range and damage range
        }

        public virtual void CleanUp()
        {
            _Entity = null;

            // Override to clear away tile indicators
        }
    }
}

