using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Core
{
    // This is the base resource for all the different types of placeable entities
    // that can exist inside the level
    public partial class LevelEntityData : Resource, ICloneable
    {
        // Identity
        public string Id;

        [Export]
        public string EntityName = "";

        [Export]
        public bool Enabled = true;

        public Vector3 TilemapPosition = Vector3.Zero;

        public Vector3 FacingDirection = Vector3.Forward;

        public Dictionary<string, Variant> DataStore = new Dictionary<string, Variant>();

        [Export]
        public PackedScene EntityScene;

        public LevelEntity CreateEntity(LevelEntityData data)
        {
            // Create instance
            var instance = EntityScene.Instantiate<LevelEntity>();

            // Set data
            instance.Data = data;

            // Return instance
            return instance;
        }

        public object Clone()
        {
            var clone = Duplicate() as LevelEntityData;

            // Level Entity Data properties
            clone.Id = Guid.NewGuid().ToString();
            // Duplicate primitives
            clone.EntityName = (string)EntityName.Clone();
            clone.FacingDirection = FacingDirection;
            clone.TilemapPosition = TilemapPosition;
            // Do not duplicate EntityScene
            clone.EntityScene = EntityScene;
            // Duplicate data store
            clone.DataStore = DataStore.Duplicate();

            return clone;
        }

        public Command SetDictValue(string key, Variant value)
        {
            return new SetDictValueCommand()
            {
                Dict = DataStore,
                Key = key,
                Value = value
            };
        }

        public Command RemoveDictKey(string key)
        {
            return new RemoveDictKeyCommand()
            {
                Dict = DataStore,
                Key = key
            };
        }

        private partial class SetDictValueCommand : Command
        {
            public Dictionary<string, Variant> Dict;
            public string Key;
            public Variant Value;

            // Undo vars
            private bool _DidUpdate = false;
            private Variant _PrevValue;

            public override void Execute()
            {
                _DidUpdate = Dict.ContainsKey(Key);
                if (_DidUpdate)
                {
                    _PrevValue = Dict[Key];
                    Dict.Remove(Key);
                }
                Dict.Add(Key, Value);

                Finish();
            }
        }

        private partial class RemoveDictKeyCommand : Command
        {
            public Dictionary<string, Variant> Dict;
            public string Key;

            // Undo vars
            private bool _DidRemove = false;
            private Variant _PrevValue;

            public override void Execute()
            {
                _DidRemove = Dict.ContainsKey(Key);
                if (_DidRemove)
                {
                    _PrevValue = Dict[Key];
                    Dict.Remove(Key);
                }

                Finish();
            }
        }
    }
}
