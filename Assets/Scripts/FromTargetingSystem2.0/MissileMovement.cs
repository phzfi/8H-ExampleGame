// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.

using Unity.Entities;
using Unity.Mathematics;

namespace FromTargetingSystem2._0
{
    [System.Serializable]
    public struct MissileMovement : IComponentData
    {
        public float3 Speed;
        public float3 CurrentSpeed;
        public float3 MaxSpeed;
        public float3 RotationSpeed;
    }
    public struct MissileTimer : IComponentData
    {
        public float Value;
    }
}