using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Tiles;

namespace ShopIsDone.Actions
{
    public partial class ArenaAction : Resource
    {
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

        [Export]
        public bool UsesEffort = false;

        [ExportGroup("Targeting")]
        [Export]
        public bool CanTargetSelf = false;

        [Export]
        public int Range = 0;

        [Export]
        public DispositionTypes TargetDisposition = 0;
        public enum DispositionTypes
        {
            Any,
            Friends,
            Enemies
        }

        [ExportGroup("Widget")]
        [Export]
        public ActionTypes ActionType = 0;
        public enum ActionTypes
        {
            Null,
            Move,
            Facing,
            Target,
            Interact,
            Task
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

        [Export]
        public bool RotateToFaceEntity = false;

        [ExportGroup("")]
        [Export(PropertyHint.MultilineText)]
        public string Description;

        // The pawn using the action
        public LevelEntity Entity;
        // The action handler
        protected ActionHandler _ActionHandler;

        /* Function to determine if this action should show up in the player's 
         * menu */
        public virtual bool IsVisibleInMenu()
        {
            return false;
        }

        /* Function for action selection either through AI or though a menu */
        public virtual bool IsAvailable()
        {
            return
                // Is the action available for the unit to use?
                _ActionHandler.IsActionAvailable(this) &&
                // Do we have the necessary components for it? Otherwise we may
                // throw an error
                HasRequiredComponents(Entity);
        }

        /* This function is for targeting. Does this curernt tile contain a valid 
         * target? */
        public virtual bool ContainsValidTarget(Tile tile)
        {
            // Check if the tile is blocked or unoccupied
            if (tile.HasObstacleOnTile || !tile.HasUnitOnTile()) return false;

            // Get the unit on the tile
            var unit = tile.UnitOnTile;

            // If the unit is not active, then they're invalid
            if (!unit.IsActive()) return false;

            // If the unit doesn't have the required components, it's an invalid
            // target
            if (!TargetHasRequiredComponents(unit)) return false;

            // Check disposition
            var teamHandler = Entity.GetComponent<TeamHandler>();
            if (TargetDisposition == DispositionTypes.Any) return true;
            else if (TargetDisposition == DispositionTypes.Friends)
            {
                return teamHandler.IsOnSameTeam(unit);
            }
            else if (TargetDisposition == DispositionTypes.Enemies)
            {
                return !teamHandler.IsOnSameTeam(unit);
            }

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

        public void SetEffort(int effort)
        {
            _ActionHandler.SetActionEffort(Id, effort);
        }

        public int GetEffortSpent()
        {
            return _ActionHandler.GetActionState(Id)?.EffortSpent ?? 0;
        }

        public virtual Dictionary<string, Variant> GetDescriptionVars()
        {
            return new Dictionary<string, Variant>();
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
                _ActionHandler.PayForAction(Id);
                _ActionHandler.SetActionUsed(Id);
                if (EndsTurnAfterUse) _ActionHandler.EndTurn();
            });
        }
    }
}
