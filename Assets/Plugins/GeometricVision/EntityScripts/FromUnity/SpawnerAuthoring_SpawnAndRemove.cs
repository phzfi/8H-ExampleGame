using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


// ReSharper disable once InconsistentNaming
namespace Plugins.GeometricVision.EntityScripts.FromUnity
{
    [RequiresEntityConversion]
    [ConverterVersion("Mikael", 1)]
    public class SpawnerAuthoring_SpawnAndRemove : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject Prefab;
        public int CountX;
        public int CountY;
        public int separationMultiplier;
        // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Prefab);
        }

        // Lets you convert the editor data representation to the entity optimal runtime representation

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new SpawnerData_SpawnAndRemove
            {
                // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                // So here we simply map the game object to an entity reference to that prefab.
                Prefab = conversionSystem.GetPrimaryEntity(Prefab),
                CountX = CountX,
                CountY = CountY,
                separationMultiplier = this.separationMultiplier
            };
            dstManager.AddComponentData(entity, spawnerData);
        }
    }
}


