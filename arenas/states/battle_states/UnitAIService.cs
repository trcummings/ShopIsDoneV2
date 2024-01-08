using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShopIsDone.AI;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas
{
    public partial class UnitAIService : UnitTurnService, IService
    {
        public IEnumerable<ActionPlanner> GetAllUnitAI()
        {
            return GetTurnEntities()
                .Where(u => u.HasComponent<ActionPlanner>())
                .Select(u => u.GetComponent<ActionPlanner>());
        }

        public void ResetAI()
        {
            foreach (var planner in GetAllUnitAI()) planner.ResetPlanner();
        }
    }
}

