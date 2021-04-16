// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable once InconsistentNaming
namespace FromTargetingSystem2._0.EntityAuthoringComponents
{
    public class SpawnerAuthoring_SpawnMissile : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        [FormerlySerializedAs("MissilePrefab")] [SerializeField] private GameObject missilePrefab = null;
        [FormerlySerializedAs("Speed")] [SerializeField] private half speed = half.zero;
        [FormerlySerializedAs("MaxSpeed")] [SerializeField] private half maxSpeed = half.zero;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(missilePrefab);
        }
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var missileSpawnerData = new MissileMovement_SpawnData()
            {
                Prefab = conversionSystem.GetPrimaryEntity(missilePrefab),
                Speed = this.speed,
                MaxSpeed = maxSpeed,
            };


            dstManager.AddComponentData(entity, missileSpawnerData);


            var timer = new MissileTimer()
            {
            };

            dstManager.AddComponentData(entity, timer);
        }
    }
}