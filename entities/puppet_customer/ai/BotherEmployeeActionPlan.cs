using Godot;
using ShopIsDone.AI;
using ShopIsDone.Utils.Commands;
using System;
using Godot.Collections;
using ShopIsDone.Widgets;
using ShopIsDone.Core;
using System.Linq;
using ActionConsts = ShopIsDone.Actions.Consts;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.Positioning;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
	public partial class BotherEmployeeActionPlan : ActionPlan
    {
        public override bool IsValid()
        {
            // Base valid check first
            if (!base.IsValid()) return false;

            return InBotherRange();
        }

        // Priority is as high as possible when we're in bother range and as low as
        // possible when not
        public override int GetPriority()
        {
            return InBotherRange() ? int.MaxValue : int.MinValue;
        }

        private bool InBotherRange()
        {
            // If blackboard doesn't have target, this isn't valid
            if (!_Blackboard.ContainsKey(Consts.ENTITY_TARGET)) return false;

            var target = (LevelEntity)_Blackboard[Consts.ENTITY_TARGET];
            var targetTile = GetTileAtTilemapPos(target.TilemapPosition);
            return TileWithinDistanceOfUs(targetTile, _Action.Range);
        }

        public override Command ExecuteAction()
        {
            // Get the target from the blackboard
            var target = (LevelEntity)_Blackboard[Consts.ENTITY_TARGET];
            var currentTile = GetTileAtTilemapPos(_Entity.TilemapPosition);

            return new SeriesCommand(
                // Show indicators on each available neighbor
                new ActionCommand(() =>
                {
                    CreateTileIndicators(
                        currentTile.FindNeighbors().Select(kv => kv.Value.TilemapPosition),
                        TileIndicator.IndicatorColor.Red
                    );
                }),
                // Wait a moment, as if deciding
                WaitFor(0.25F),
                // Briefly hide the indicators
                new ActionCommand(ClearTileIndicators),
                new WaitIdleFrameCommand(_Entity),
                // Show indicator only over target
                new ActionCommand(() =>
                {
                    CreateTileIndicators(
                        new Array<Vector3>() { target.TilemapPosition },
                        TileIndicator.IndicatorColor.Red
                    );
                }),
                // Rotate towards target
                new ActionCommand(() => _Entity.FacingDirection = _Entity.GetFacingDirTowards(target.GlobalPosition)),
                // Wait another moment as if confirming
                WaitFor(0.5F),
                // Hide indicators
                new ActionCommand(ClearTileIndicators),
                // Bother
                new DeferredCommand(() => _ActionService.ExecuteAction(_Action, new Dictionary<string, Variant>()
                {
                    { ActionConsts.TARGET, target },
                    // Get positioning of action
                    { ActionConsts.POSITIONING, (int)Positioning.GetPositioning(_Entity.FacingDirection, target.FacingDirection) }
                })),
                // Set blackboard values
                new DeferredCommand(() => new ActionCommand(() =>
                {
                    _Blackboard.Add(Consts.BOTHERED_EMPLOYEE, true);
                }))
            );
        }
    }
}
