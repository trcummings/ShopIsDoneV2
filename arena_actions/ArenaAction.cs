using Godot;
using ShopIsDone.Arenas;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System;

namespace ShopIsDone.Actions
{
    public partial class ArenaAction : Resource
    {
        [Export]
        public string Id;

        [Export]
        public string ActionName;

        [Export]
        public bool EndsTurnAfterUse = false;

        [Export]
        public bool CanBeUsedMultipleTimes = false;

        [Export]
        public int ActionCost = 0;

        [Export]
        public int ExcessAPCost = 0;

        [Export]
        public bool CanTargetSelf = false;

        [Export]
        public int Range = 0;

        public enum ActionTypes
        {
            Null,
            Move,
            Facing,
            Target,
            Inspect
        }
        [Export]
        public ActionTypes ActionType = 0;

        public enum IndicatorTypes
        {
            Neutral,
            Friendly,
            Hostile
        }
        [Export]
        public IndicatorTypes IndicatorType = 0;

        public virtual bool HasRequiredComponents(LevelEntity entity)
        {
            return true;
        }

        public virtual bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return true;
        }

        // The pawn using the action
        public LevelEntity Entity;
        private ActionHandler _ActionHandler;

        public void Init(ActionHandler actionHandler)
        {
            _ActionHandler = actionHandler;
            Entity = _ActionHandler.Entity;
        }

        public virtual Command Execute(Dictionary<string, Variant> message = null)
        {
            return new ActionCommand(() =>
            {
                _ActionHandler.SetActionUsed(Id);
                if (EndsTurnAfterUse) _ActionHandler.EndTurn();
            });
        }
    }
}
