using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Tiles;

namespace ShopIsDone.Actions
{
    public partial class ArenaAction : Resource
    {
        [ExportGroup("Identification")]
        [Export]
        public string Id;

        [Export]
        public string ActionName;

        [ExportGroup("Usage")]
        [Export]
        public bool EndsTurnAfterUse = false;

        [Export]
        public bool CanBeUsedMultipleTimes = false;

        [ExportGroup("Action Cost")]
        [Export]
        public int ActionCost = 0;

        [Export]
        public int ExcessAPCost = 0;

        [ExportGroup("Targeting")]
        [Export]
        public bool CanTargetSelf = false;

        [Export]
        public int Range = 0;

        [ExportGroup("Widget")]
        [Export]
        public ActionTypes ActionType = 0;
        public enum ActionTypes
        {
            Null,
            Move,
            Facing,
            Target
        }

        [Export]
        public IndicatorTypes IndicatorType = 0;
        public enum IndicatorTypes
        {
            Neutral,
            Friendly,
            Hostile
        }

        [ExportGroup("Camera Behavior")]
        [Export]
        public bool FollowEntity = false;

        [Export]
        public bool FocusEntity = false;

        [ExportGroup("Action Menu")]
        [Export]
        public string MenuTitle = "";

        [Export(PropertyHint.MultilineText)]
        public string MenuDescription = "";

        // The pawn using the action
        public LevelEntity Entity;
        // The action handler
        protected ActionHandler _ActionHandler;

        // Functions for action selection either through AI or though a menu
        public virtual bool IsVisibleInMenu()
        {
            return false;
        }

        public virtual bool IsAvailable()
        {
            return false;
        }

        /* This function is for targeting. Does this curernt tile contain a valid 
         * target? */
        public virtual bool ContainsValidTarget(Tile tile)
        {
            return false;
        }

        public virtual bool HasRequiredComponents(LevelEntity entity)
        {
            return true;
        }

        public virtual bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return true;
        }

        public virtual void Init(ActionHandler actionHandler)
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
