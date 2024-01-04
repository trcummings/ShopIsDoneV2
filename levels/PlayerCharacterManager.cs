using Godot;
using ShopIsDone.Actors;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Core;

namespace ShopIsDone.Levels
{
    // This service creates the actors that the player will control in a level,
    // it also manages their transitions between arena state, free move, who
    // should be following who, and so on
    public partial class PlayerCharacterManager : Node3D, IService
    {
        [Export]
        private PackedScene _HaskellScene;

        [Export]
        private Node3D _DefaultSpawnPoint;

        private Actor _Haskell;

        public void Init()
        {
            _Haskell = _HaskellScene.Instantiate<Actor>();
            AddChild(_Haskell);

            // Move unit to the default spawn point
            _Haskell.GlobalTransform = _DefaultSpawnPoint.GlobalTransform;
        }

        public void EnterArena()
        {
            _Haskell.Init(new ActorInput());
            _Haskell.EnterArena();
        }

        public void ExitArena()
        {
            _Haskell.ExitArena();
        }

        public void SetLeaderPlayerInput(PlayerActorInput input)
        {
            // Give the leader player input to let them move around freely
            _Haskell.Init(input);
        }

        public Array<LevelEntity> GetAllUnits()
        {
            return new Array<LevelEntity>() { _Haskell };
        }

        public LevelEntity GetLeader()
        {
            return _Haskell;
        }
    }
}
