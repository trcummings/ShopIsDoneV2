using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Arenas.ArenaScripts
{
    // This is kind of like a subclass sandbox for interacting with the world
    public partial class ArenaScript : Resource
    {
        // Reference to the arena
        protected Arena Arena;
        private EntityManager _EntityManager;

        public virtual void Init(Arena arena, EntityManager entityManager)
        {
            Arena = arena;
            _EntityManager = entityManager;
        }

        public virtual Command ExecuteScript()
        {
            return new Command();
        }

        //protected LevelEntity GetLevelEntity(string id)
        //{
        //    return _EntityManager.GetEntity<LevelEntity>(id);
        //}

        //protected Pawn GetPawnById(string id)
        //{
        //    return Arena.GetAllPawns().Where(e => e.Id == id).First();
        //}

        //protected Interactable GetInteractableById(string id)
        //{
        //    return Arena.GetAllInteractables().Where(e => e.Id == id).First();
        //}

        //protected Tile FindTileByOffset(Tile baseTile, Vector3 offset)
        //{
        //    return Arena
        //        .GetAllTiles()
        //        .Where(t => t.TilemapPosition == (baseTile.TilemapPosition + offset))
        //        .First();
        //}

        //protected Command MovePawnTo(Pawn pawn, Tile goalTile)
        //{
        //    var movement = pawn.GetComponent<MovementComponent>();
        //    var actions = pawn.GetComponent<ActionHandlerComponent>();
        //    // Find the best path to the destination
        //    // HACK: Use impossibly large pawn move range to 100000
        //    var bestPath = new SimpleTileAStar().GetMovePath(movement.GetAvailableMoves(null, true, 100000), pawn.CurrentTile, goalTile);
        //    // Add current tile to the path
        //    bestPath = bestPath.Prepend(pawn.CurrentTile).ToList();
        //    // Follow the path
        //    var action = actions.GetPawnAction("walk");
        //    return action.Execute(Arena, new Dictionary<string, object>()
        //    {
        //        { "MovePath", new Godot.Collections.Array(bestPath) }
        //    });
        //}

        //protected Command RunAnimation(LevelEntity entity, string animName)
        //{
        //    return entity.GetComponent<RenderComponent>()?.PerformAction(animName) ?? new Command();
        //}

        //protected CharacterData GetCharacter(string id)
        //{
        //    return Arena.GetCharacter(id);
        //}
    }

    public partial class CommandArenaScript : ArenaScript
    {
        public Func<Command> CommandFn;

        public override Command ExecuteScript()
        {
            return CommandFn.Invoke();
        }
    }
}