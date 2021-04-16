// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.
using Plugins.GeometricVision;
using Unity.Entities;

namespace FromTargetingSystem2._0.EntityComponents
{
    [System.Serializable]
    public struct MissileTargetLock : IComponentData
    {
        public GeometryDataModels.Target Target;
    }
}