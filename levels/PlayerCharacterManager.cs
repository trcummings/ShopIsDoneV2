using Godot;
using ShopIsDone.Actors;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;
using System.Linq;

namespace ShopIsDone.Levels
{
    // This service creates the actors that the player will control in a level,
    // it also manages their transitions between arena state, free move, who
    // should be following who, and so on
    public partial class PlayerCharacterManager : Node3D, IService
    {
        [Export]
        private PackedScene _LeaderScene;

        [Export]
        private Array<PackedScene> _FollowerScenes = new Array<PackedScene>();

        [Export]
        private DirectionalPoint _DefaultSpawnPoint;

        private int _LeaderLayer = 10;

        private Actor _Leader;
        private Array<Actor> _Followers = new Array<Actor>();

        public void Init()
        {
            // Create leader
            _Leader = _LeaderScene.Instantiate<Actor>();
            AddChild(_Leader);

            // Create followers
            foreach (var scene in _FollowerScenes)
            {
                var follower = scene.Instantiate<Actor>();
                AddChild(follower);
                _Followers.Add(follower);
            }

            // Move leader to the default spawn point
            WarpGroupToPosition(
                _DefaultSpawnPoint.GlobalPosition,
                _DefaultSpawnPoint.FacingDirection
            );

            // Set leader layer
            SetLeaderLayer();

            // Idle them
            Idle();
        }

        /* NB: Position is GLOBAL */
        public void WarpGroupToPosition(Vector3 position, Vector3 facingDir)
        {
            // Move leader to the given position
            _Leader.GlobalPosition = position;
            _Leader.FacingDirection = facingDir;
            // Spawn followers nearby
            for (int i = 0; i < _Followers.Count; i++)
            {
                var follower = _Followers[i];
                follower.GlobalPosition = position + (facingDir.Reflect(Vector3.Up) * (i + 1));
            }
        }

        private void SetLeaderLayer()
        {
            _Leader.SetCollisionLayerValue(_LeaderLayer, true);
            foreach (var follower in _Followers) follower.SetCollisionLayerValue(_LeaderLayer, false);
        }

        public void Idle()
        {
            // Idle each one
            foreach (var unit in GetAllUnits()) unit.Idle();
        }

        public void EnterArena()
        {
            // Have all units init and enter arena mode
            foreach (var unit in GetAllUnits()) unit.EnterArena();
        }

        public void MoveFreely(PlayerActorInput input)
        {
            // Get all units
            var units = GetAllUnits();

            // If there's only one unit, that one is the leader
            if (units.Count == 1)
            {
                units.First().MoveFreely(input);
                return;
            }

            // Otherwise, have each unit follow the previous person
            foreach (var (prev, current) in units.WithPrevious())
            {
                if (prev == null) continue;
                // Give the leader player input to let them move around freely
                if (prev == _Leader) prev.MoveFreely(input);
                // Make each unit follow the one in front of them
                if (current != _Leader) current.FollowLeader(prev);
            }
        }

        public Array<Actor> GetAllUnits()
        {
            var units = new Array<Actor>() { _Leader };
            units.AddRange(_Followers);
            return units;
        }

        public LevelEntity GetLeader()
        {
            return _Leader;
        }
    }
}
