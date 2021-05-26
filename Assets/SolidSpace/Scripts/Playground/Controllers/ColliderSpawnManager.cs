using System.Collections.Generic;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground
{
    public class ColliderSpawnManager : IController
    {
        private struct ColliderInfo
        {
            public float2 position;
            public Entity entity;
        }

        public EControllerType ControllerType => EControllerType.Playground;

        private readonly IEntityWorldManager _entityManager;
        private readonly ColliderSpawnManagerConfig _config;
        private readonly Camera _camera;
        private readonly List<ColliderInfo> _spawnedEntities;
        private readonly ComponentType[] _archetype;
        
        public ColliderSpawnManager(IEntityWorldManager entityManager, ColliderSpawnManagerConfig config, Camera camera)
        {
            _entityManager = entityManager;
            _config = config;
            _camera = camera;
            _spawnedEntities = new List<ColliderInfo>();
            _archetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent),
                typeof(RotationComponent),
                typeof(SizeComponent)
            };
        }
        
        public void InitializeController()
        {
            for (var i = 0; i < _config.OnStartSpawnCount; i++)
            {
                var x = Random.Range(_config.SpawnRangeX.x, _config.SpawnRangeX.y);
                var y = Random.Range(_config.SpawnRangeY.x, _config.SpawnRangeY.y);
                var samplePosition = new Vector2(x, y);
                
                for (var j = 0; j < _config.SpawnPerSpawn; j++)
                {
                    var finalPosition = samplePosition + Random.insideUnitCircle * _config.SpawnExtraRadius;
                    Spawn(finalPosition);
                }
            }
        }

        public void UpdateController()
        {
            if (!_config.MouseControl)
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0) && GetClickPosition(out var clickPosition))
            {
                Spawn(clickPosition);
            }

            if (Input.GetMouseButtonDown(1) && GetClickPosition(out clickPosition))
            {
                Destroy(clickPosition);
            }
        }
        
        private void Spawn(float2 position)
        {
            var width = (half) Random.Range(_config.ColliderWidth.x, _config.ColliderWidth.y);
            var height = (half) Random.Range(_config.ColliderHeight.x, _config.ColliderHeight.y);
            var info = new ColliderInfo
            {
                position = position,
                entity = _entityManager.CreateEntity(_archetype),
            };
            _spawnedEntities.Add(info);
            
            _entityManager.SetComponentData(info.entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(info.entity, new SizeComponent
            {
                value = new half2(width, height)
            });
            _entityManager.SetComponentData(info.entity, new RotationComponent
            {
                value = (half) Random.value
            });
        }

        private void Destroy(float2 position)
        {
            if (_spawnedEntities.Count == 0)
            {
                return;
            }
            
            var minIndex = -1;
            var minValue = float.MaxValue;
            for (var i = 0; i < _spawnedEntities.Count; i++)
            {
                var distance = FloatMath.Distance(_spawnedEntities[i].position, position);
                if (distance < minValue)
                {
                    minValue = distance;
                    minIndex = i;
                }
            }

            var info = _spawnedEntities[minIndex];
            _spawnedEntities.RemoveAt(minIndex);
            _entityManager.DestroyEntity(info.entity);
        }

        private bool GetClickPosition(out float2 clickPosition)
        {
            clickPosition = float2.zero;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(-Vector3.forward, Vector3.zero);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }
            
            var hitPos = ray.origin + ray.direction * distance;
            clickPosition = new float2
            {
                x = hitPos.x,
                y = hitPos.y
            };
            
            return true;
        }

        public void FinalizeController()
        {
            
        }
    }
}