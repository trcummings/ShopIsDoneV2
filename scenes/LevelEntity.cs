using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Tiles;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Utils;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Arenas;

namespace ShopIsDone.Core
{
    public interface IComponentContainer
    {
        List<IComponent> Components { get; }

        C GetComponent<C>() where C : IComponent;

        bool HasComponent<C>() where C : IComponent;
    }

    public partial class LevelEntity : Node3D, IComponentContainer
    {
        [Signal]
        public delegate void ChangedFacingDirectionEventHandler(Vector3 newDir);

        [Signal]
        public delegate void EntityEnabledEventHandler();

        [Signal]
        public delegate void EntityDisabledEventHandler();

        [Signal]
        public delegate void InitializedEventHandler();

        [Export]
        protected ArenaEvents _ArenaEvents;

        // A reference to an ID that contains a data template for this specific entity
        [Export]
        public string DataTemplateId
        {
            get { return _DataTemplateId; }
            set { _DataTemplateId = value; }
        }
        private string _DataTemplateId;

        // Entity Data
        private LevelEntityData _Data;
        public LevelEntityData Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        // Id
        public string Id
        {
            get { return Data.Id; }
        }

        // IsActive
        public bool Enabled
        {
            get { return Data.Enabled; }
        }

        // Name
        public string EntityName
        {
            get { return Data?.EntityName ?? ""; }
            set { Data.EntityName = value; }
        }

        // Facing Direction
        public Vector3 FacingDirection
        {
            get { return Data.FacingDirection; }
            set
            {
                Data.FacingDirection = value;
                EmitSignal(nameof(ChangedFacingDirection), Data.FacingDirection);
            }
        }

        // Tilemap position
        public Vector3 TilemapPosition
        {
            get { return Data.TilemapPosition; }
            set { Data.TilemapPosition = value; }
        }

        #region Components
        protected List<IComponent> _Components = new List<IComponent>();
        public List<IComponent> Components { get { return _Components; } }

        public C GetComponent<C>() where C : IComponent
        {
            return (C)_Components.Find(c => c is C);
        }

        public bool HasComponent<C>() where C : IComponent
        {
            return _Components.Any(c => c is C);
        }

        private void AddComponent(IComponent component)
        {
            _Components.Add(component);
            // Set entity
            component.Entity = this;
        }
        #endregion

        // Reference other entities
        protected EntityManager _EntityManager;

        public override void _Ready()
        {
            base._Ready();

            // Connect to enabled/disabled hooks
            EntityEnabled += OnEnabled;
            EntityDisabled += OnDisabled;
            ChangedFacingDirection += OnChangedFacingDir;
        }

        // Init
        public virtual Command Init(EntityManager entityManager)
        {
            return new ActionCommand(() =>
            {
                // Set entity manager
                _EntityManager = entityManager;

                // Get all components in the children of the entity
                foreach (var component in GetChildren().OfType<IComponent>())
                {
                    AddComponent(component);
                }

                // Register whole entity
                _EntityManager.AddEntity(this);

                if (Enabled) OnEnabled();
                else OnDisabled();

                // Initialize components with entity manager
                foreach (var component in _Components.OfType<IInitializableComponent>())
                {
                    component.Init(_EntityManager);
                }

                // Emit initialized signal
                EmitSignal(nameof(Initialized));
            });
        }

        // Positioning
        public virtual void SetGlobalPosition(Vector3 pos)
        {
            GlobalPosition = pos;
        }

        // Facing Direction
        public Command SetFacingDirection(Vector3 facingDir)
        {
            return new SetFacingDirCommand()
            {
                Entity = this,
                FacingDir = facingDir
            };
        }

        private void OnChangedFacingDir(Vector3 newDir)
        {
            GlobalRotation = Vector3.Up * Vec3.FacingDirToYRad(newDir);
        }

        // Enabled
        public Command SetEnabled(bool value)
        {
            return new SetEnabledCommand()
            {
                Entity = this,
                Value = value
            };
        }

        protected virtual void OnEnabled()
        {
            Show();
        }

        protected virtual void OnDisabled()
        {
            Hide();
        }

        // Is On Tile
        public virtual bool IsOnTile(Tile tile)
        {
            return tile?.TilemapPosition == TilemapPosition;
        }

        // Update function
        public virtual Command Update()
        {
            return new SeriesCommand(
                _Components
                    .OfType<IUpdateOnActionComponent>()
                    .Select(c => new DeferredCommand(c.UpdateOnAction))
                    .ToArray()
            );
        }

        // In arena / available
        private List<IEntityActiveHandler> _ActiveHandlers;
        public virtual bool IsInArena()
        {
            if (_ActiveHandlers == null)
            {
                _ActiveHandlers = GetChildren().OfType<IEntityActiveHandler>().ToList();
            }

            // Default to true
            if (_ActiveHandlers.Count == 0) return true;
            // Iterate through active handlers
            return _ActiveHandlers.All(h => h.IsInArena());
        }

        public virtual bool IsActive()
        {
            if (_ActiveHandlers == null)
            {
                _ActiveHandlers = GetChildren().OfType<IEntityActiveHandler>().ToList();
            }

            // Default to true
            if (_ActiveHandlers.Count == 0) return true;
            // Iterate through active handlers
            return _ActiveHandlers.All(h => h.IsActive());
        }

        #region ArenaScripts
        public void EmitArenaScript(ArenaScript script)
        {
            _ArenaEvents.AddArenaCommandToQueue(script);
        }
        #endregion

        #region DataStore
        public IEnumerable<string> GetDataStoreKeys()
        {
            return Data.DataStore.Keys;
        }

        public Command SetDataStoreValue(string key, Variant value)
        {
            return Data.SetDictValue(key, value);
        }

        public Command RemoveDataStoreValue(string key)
        {
            return Data.RemoveDictKey(key);
        }

        public bool HasDataStoreValue(string key)
        {
            return Data.DataStore.ContainsKey(key);
        }

        public Variant GetDataStoreValue(string key, Variant defaultValue = default)
        {
            return Data.DataStore.GetValueOrDefault(key, defaultValue);
        }
        #endregion

        // Commands
        private partial class SetFacingDirCommand : Command
        {
            public LevelEntity Entity;
            public Vector3 FacingDir;

            public override void Execute()
            {
                // Set facing dir
                Entity.FacingDirection = FacingDir;

                // Finish
                Finish();
            }
        }

        private partial class SetEnabledCommand : Command
        {
            public LevelEntity Entity;
            public bool Value;

            public override void Execute()
            {
                // Only update if the value changed
                if (Value == Entity.Data.Enabled) 
                {
                    Finish();
                    return;
                }

                // Otherwise do an update
                if (Value) Entity.EmitSignal(nameof(EntityEnabled));
                else Entity.EmitSignal(nameof(EntityDisabled));

                // Set enabled
                Entity.Data.Enabled = Value;

                // Finish
                Finish();
            }
        }
    }
}
