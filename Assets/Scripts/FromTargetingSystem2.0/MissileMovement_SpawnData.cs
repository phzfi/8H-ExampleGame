// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.

using Unity.Entities;
using Unity.Mathematics;

namespace FromTargetingSystem2._0
{
    public struct MissileMovement_SpawnData : IComponentData
    {
        
        public Entity Prefab;
        public float3 Speed;
        public float3 MaxSpeed;
        public float3 Direction;
        public int4 TargetingSystemIndex;
        public float4 ExplosionTargetTakeDownRadius;
    }

}