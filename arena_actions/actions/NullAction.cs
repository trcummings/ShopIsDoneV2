using System;
using ShopIsDone.Core;

namespace ShopIsDone.Actions
{
    public partial class NullAction : ArenaAction
    {
        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return false;
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return false;
        }
    }
}