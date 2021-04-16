// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.
using Unity.Entities;
using Unity.Mathematics;

namespace FromTargetingSystem2._0.EntityComponents
{
    [System.Serializable]

    public struct Explosion : IComponentData
    {
        public bool4 Exploded;
        public float4 Duration;
        public float3 position;
        public float3 direction;
        public float3 force;
    }
}
