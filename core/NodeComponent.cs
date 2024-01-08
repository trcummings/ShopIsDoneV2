using System;
using Godot;

namespace ShopIsDone.Core
{
    [Tool]
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

        public virtual void Init()
        {

        }
    }
}

