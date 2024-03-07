﻿using System;
using Godot;
using Godot.Collections;
using SystemCollections = System.Collections.Generic;
using System.Linq;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Core
{
    public interface IComponentContainer
    {
        SystemCollections.List<IComponent> Components { get; }

        C GetComponent<C>() where C : IComponent;

        bool HasComponent<C>() where C : IComponent;
    }

    public interface ILevelEntity : IComponentContainer
    {
        public Vector3 TilemapPosition { get; }

        public Vector3 FacingDirection { get; }

        public Vector3 GlobalPosition { get; }
    }

    [Tool]
    public partial class LevelEntity : CharacterBody3D, IComponentContainer
    {
        [Signal]
        public delegate void EntityEnabledEventHandler();

        [Signal]
        public delegate void EntityDisabledEventHandler();

        [Signal]
        public delegate void InitializedEventHandler();

        // Id
        [Export]
        public string Id;

        // IsActive
        [Export]
        public bool Enabled = true;

        // Name
        [Export]
        public string EntityName;

        [Export]
        public Dictionary<string, Variant> DataStore = new Dictionary<string, Variant>();

        [Export]
        private NodePath _TilemapPositionHandlerPath;
        private TilemapPositionHandler _TilemapPositionHandler;

        [Export]
        private NodePath _FacingDirectionHandlerPath;
        private FacingDirectionHandler _FacingDirectionHandler;

        [Export]
        public Node3D WidgetPoint;

        // Tilemap position
        public Vector3 TilemapPosition
        {
            get { return _TilemapPositionHandler?.TilemapPosition ?? Vector3.Inf; }
        }

        #region Dir Tool Export
        [Export]
        private DirEnum.Dir EditorFacingDir
        {
            get { return DirEnum.VectorToDir(FacingDirection); }
            set {
                _EditorFacingDir = DirEnum.DirToVector(value);
                FacingDirection = DirEnum.DirToVector(value);
            }
        }
        private Vector3 _EditorFacingDir = Vector3.Forward;
        #endregion

        public Vector3 FacingDirection
        {
            get
            {
                return _FacingDirectionHandler?.FacingDirection ?? Vector3.Forward;
            }
            set
            {
                if (_FacingDirectionHandler != null)
                {
                    _FacingDirectionHandler.FacingDirection = value;
                }
            }
        }

        #region Components
        protected SystemCollections.List<IComponent> _Components = new SystemCollections.List<IComponent>();
        public SystemCollections.List<IComponent> Components { get { return _Components; } }

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

        public override void _Ready()
        {
            base._Ready();
            _TilemapPositionHandler = GetNode<TilemapPositionHandler>(_TilemapPositionHandlerPath);
            _FacingDirectionHandler = GetNode<FacingDirectionHandler>(_FacingDirectionHandlerPath);
            _FacingDirectionHandler.FacingDirection = _EditorFacingDir;

            // Connect to enabled/disabled hooks
            EntityEnabled += OnEnabled;
            EntityDisabled += OnDisabled;

            // If we don't have a widget point, set it to the entity itself
            WidgetPoint ??= this;

            // Get all components in the children of the entity
            foreach (var component in GetChildren().OfType<IComponent>())
            {
                AddComponent(component);
            }
        }

        // Init
        public virtual void Init()
        {
            if (Enabled) OnEnabled();
            else OnDisabled();

            // Initialize components
            foreach (var component in _Components) component.Init();

            // Emit initialized signal
            EmitSignal(nameof(Initialized));
        }

        // Positioning
        public virtual void SetGlobalPosition(Vector3 pos)
        {
            GlobalPosition = pos;
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

        public virtual Command OnCleanUp()
        {
            return new SeriesCommand(
                _Components
                    .OfType<IOnCleanupComponent>()
                    .Select(c => new DeferredCommand(c.OnCleanup))
                    .ToArray()
            );
        }

        // In arena / available
        private SystemCollections.List<IEntityActiveHandler> _ActiveHandlers;
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

        // Commands
        private partial class SetEnabledCommand : Command
        {
            public LevelEntity Entity;
            public bool Value;

            public override void Execute()
            {
                // Only update if the value changed
                if (Value == Entity.Enabled) 
                {
                    Finish();
                    return;
                }

                // Otherwise do an update
                if (Value) Entity.EmitSignal(nameof(EntityEnabled));
                else Entity.EmitSignal(nameof(EntityDisabled));

                // Set enabled
                Entity.Enabled = Value;

                // Finish
                Finish();
            }
        }
    }
}
