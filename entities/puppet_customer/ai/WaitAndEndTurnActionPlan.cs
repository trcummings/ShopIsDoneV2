using Godot;
using ShopIsDone.AI;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Extensions;
using System;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
	public partial class WaitAndEndTurnActionPlan : ActionPlan
    {
        public override Command ExecuteAction()
        {
            return new SeriesCommand(
                new ActionCommand(() =>
                {
                    // If we have a tile target, make us face that direction
                    if (_Blackboard.ContainsKey(Consts.TILE_TARGET))
                    {
                        var tileTarget = (Tile)_Blackboard[Consts.TILE_TARGET];
                        _Entity.FacingDirection = _Entity.GetFacingDirTowards(tileTarget.TilemapPosition);
                    }
                }),
                _ActionService.ExecuteAction(_Action)
            );
        }
    }
}
