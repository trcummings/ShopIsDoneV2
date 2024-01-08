using System;
using Godot;

namespace ShopIsDone.Conditions
{
    /** 
	A condition is like a "query" on the current state of the game world. It's
	tracked by a system that keeps track of what conditions are complete and
	what aren't.
	 */
    public partial class Condition : Node
    {
        [Export]
        private string _Id = "";
        // Public accessor
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [Export]
        public bool IsVisibleInUI;

        [Export]
        public string Verb = "[Verb]";

        [Export]
        public string Noun = "[Noun]";

        [Export]
        public string PluralNoun = "[Nouns]";

        [Export]
        public string Article = "a";

        public string Description { get { return GetDescription(); } }

        protected ConditionsService _ConditionsService;

        public virtual bool IsComplete()
        {
            return false;
        }

        public virtual string GetDescription()
        {
            return Verb + " " + Article + " " + Noun;
        }

        // This is just for our composite conditions to overwrite
        public virtual bool HasCondition(Condition condition)
        {
            return condition == this;
        }

        // Initializing the entity in the entity manager
        public virtual void Init(ConditionsService conditionsService)
        {
            _ConditionsService = conditionsService;
        }
    }
}

