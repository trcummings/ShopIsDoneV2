using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Core
{
    // This component is purely for RUNTIME accessing of entities in the game, it
    // cannot and will not ever touch any saved data. All data for entities must be
    // dynamically initialized when the entity is added to the scene for it to be
    // accessible
    public partial class EntityManager : Node
    {
        private Dictionary<string, LevelEntity> _Entities = new Dictionary<string, LevelEntity>();
        private Dictionary<Type, HashSet<string>> _EntitiesByComponents = new Dictionary<Type, HashSet<string>>();

        public void AddEntity<T>(T entity) where T : LevelEntity
        {
            // Check for duplicate key
            if (_Entities.ContainsKey(entity.Id))
            {
                GD.PrintErr($"Id Collision! {entity.Id} already exists!");
            }

            // Add entity to runtime DB
            _Entities.Add(entity.Id, entity);

            // Add components to runtime DB
            foreach (var component in entity.Components)
            {
                RegisterComponentWithEntity(entity.Id, component);
            }
        }

        private void RegisterComponentWithEntity(string entityId, IComponent component)
        {
            var cType = component.GetType();
            // Initialize hashset if it doesn't exist yet
            if (!_EntitiesByComponents.ContainsKey(cType))
            {
                _EntitiesByComponents.Add(cType, new HashSet<string>());
            }
            // Add component to component dictionary
            _EntitiesByComponents[cType].Add(entityId);
        }

        public T GetEntity<T>(string entityId) where T : LevelEntity
        {
            if (_Entities.TryGetValue(entityId, out LevelEntity entity))
            {
                return entity as T;
            }
            return null;
        }

        public void RemoveEntity<T>(string entityId) where T : LevelEntity
        {
            // Catch bad Ids
            if (string.IsNullOrEmpty(entityId) || !_Entities.ContainsKey(entityId)) return;

            // Pull entity
            var entity = _Entities[entityId];

            // Remove from runtime DB
            _Entities.Remove(entityId);

            // Remove components from runtime DB
            foreach (var component in entity.Components)
            {
                _EntitiesByComponents[component.GetType()].Remove(entity.Id);
            }
        }

        public IEnumerable<C> GetAllComponentsOfType<C>() where C : IComponent
        {
            return GetEntitiesByComponent<C>().Select(e => e.GetComponent<C>());
        }

        public IEnumerable<LevelEntity> GetEntitiesByComponent<C>() where C : IComponent
        {
            var cType = typeof(C);
            // Return empty if no hash set for that component
            if (!_EntitiesByComponents.ContainsKey(cType)) return new List<LevelEntity>();

            // Otherwise, map across entities to get components
            return _EntitiesByComponents[cType].Select(id => _Entities[id]);
        }

        public List<T> GetAllEntities<T>() where T : LevelEntity
        {
            return _Entities.Values.OfType<T>().ToList();
        }
    }
}