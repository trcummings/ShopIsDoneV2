using Godot;
using ShopIsDone.AI;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
	public partial class IntimidateActionPlan : ActionPlan
    {
        // Always as high priority as possible so we do it first
        public override int GetPriority()
        {
            return int.MaxValue;
        }
    }
}
