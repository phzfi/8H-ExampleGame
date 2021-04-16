// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.
using Unity.Entities;
using Unity.Mathematics;

// ReSharper disable once InconsistentNaming
namespace FromTargetingSystem2._0.EntityComponents
{
    [System.Serializable]
    public struct StreamMovement_MoveStreamComponent : IComponentData
    {
        public int SpawnIndex;
        public float Speed;
        public float3 Direction;
    }
}
