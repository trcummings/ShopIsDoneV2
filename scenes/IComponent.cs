using Godot;
using ShopIsDone.Utils.Commands;
using System;

namespace ShopIsDone.Core
{
    public interface IComponent
    {
        LevelEntity Entity { get; set; }

        C GetComponent<C>() where C : IComponent;
    }

    public interface IUpdateOnActionComponent : IComponent
    {
        Command UpdateOnAction();
    }

    public interface IInitializableComponent : IComponent
    {
        void Init(EntityManager entityManager);
    }

    public partial class NodeComponent : Node, IComponent
    {
        public LevelEntity Entity
        {
            get { return _Entity; }
            set { _Entity = value; }
        }
        private LevelEntity _Entity;

        public C GetComponent<C>() where C : IComponent
        {
            return _Entity.GetComponent<C>();
        }
    }

    public partial class SpatialComponent : Node3D, IComponent
    {
        public LevelEntity Entity
        {
            get { return _Entity; }
            set { _Entity = value; }
        }
        private LevelEntity _Entity;

        public C GetComponent<C>() where C : IComponent
        {
            return _Entity.GetComponent<C>();
        }
    }
}