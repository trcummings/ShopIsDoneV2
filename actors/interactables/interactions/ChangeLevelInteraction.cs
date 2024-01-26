using Godot;
using ShopIsDone.Core.Data;

namespace ShopIsDone.Interactables.Interactions
{
    [Tool]
	public partial class ChangeLevelInteraction : Interaction
	{
        [Export]
        private string _ToLevel;

        private Events _Events;
        private LevelDb _LevelDb;

        public override void _Ready()
        {
            base._Ready();
            _Events = Events.GetEvents(this);
            _LevelDb = LevelDb.GetLevelDb(this);
        }

        public override void Execute()
        {
            _Events.RequestLevelChange(_ToLevel);

            Finish();
        }

        public override string[] _GetConfigurationWarnings()
        {
            if (!_LevelDb.HasLevelWithId(_ToLevel))
            {
                return new string[] { $"No level with Id {_ToLevel} found in LevelDb!" }; 
            }
            return base._GetConfigurationWarnings();
        }
    }
}
