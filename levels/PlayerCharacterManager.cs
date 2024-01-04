using Godot;
using ShopIsDone.Actors;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;

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
        private Node3D _DefaultSpawnPoint;

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
            _Leader.GlobalPosition = _DefaultSpawnPoint.GlobalPosition;
            // Spawn followers nearby
            for(int i = 0; i < _Followers.Count; i++)
            {
                var follower = _Followers[i];
                follower.GlobalPosition = _DefaultSpawnPoint.GlobalPosition + (Vector3.Left * (i + 1));
            }

            // Idle them
            Idle();
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
            // Have each follower follow the previous person
            foreach (var (prev, current) in GetAllUnits().WithPrevious())
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
