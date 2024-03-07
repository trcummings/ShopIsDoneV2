using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Levels;
using ShopIsDone.Utils;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using System;
using System.Linq;

namespace ShopIsDone.Arenas
{
    public partial class ArenaEntitiesService : Node, IService, IInitializable
    {
        [Export]
        private Arena _Arena;

        [Inject]
        private PlayerCharacterManager _PlayerCharacterManager;

        public void Init()
        {
            // Inject
            InjectionProvider.Inject(this);
        }

        /* Gets us entities in the arena that are strictly within the Arena, e.g. 
         * not any player units */
        public Array<LevelEntity> GetArenaChildEntities()
        {
            return GetTree()
                .GetNodesInGroup("entities")
                .OfType<LevelEntity>()
                .Where(_Arena.IsAncestorOf)
                .ToGodotArray();
        }

        /* This gets us ALL entities that are of concern to the Arena. NOTE WELL, 
         * this does not account for entity state like IsInArena / IsActive. It 
         * is EVERY single entity of concern to the Arena. Do that filtering 
         * further down the line
         */
        public Array<LevelEntity> GetAllArenaEntities()
        {
            var allEntities = GetArenaChildEntities().ToList();

            // Add player characters to the mix
            allEntities.AddRange(_PlayerCharacterManager.GetAllUnits());

            return allEntities.ToGodotArray();
        }
    }
}

