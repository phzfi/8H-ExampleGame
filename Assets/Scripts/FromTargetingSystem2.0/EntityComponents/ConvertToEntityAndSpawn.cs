// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.
using System;
using Plugins.GeometricVision;
using Unity.Entities;

namespace FromTargetingSystem2._0.EntityComponents
{
    [AlwaysUpdateSystem]
    [DisableAutoCreation]
    public class ConvertToEntityAndSpawn : SystemBase
    {

        private int amountToSpawn;
        private bool runOnce;
        private Type entityFilter;

        private EntityManager entityManager;
        private Entity prefabEntity;

        protected override void OnCreate()
        {
            runOnce = true;
            entityManager = EntityManager;
        }

        protected override void OnUpdate()
        {
            if (runOnce)
            {
                for (int index = 0; index < amountToSpawn; index++)

                {
                    prefabEntity = entityManager.Instantiate(prefabEntity);
                    entityManager.AddComponent(prefabEntity, typeof(GeometryDataModels.Target));
                    entityManager.AddComponent(prefabEntity, entityFilter);
                    entityManager.AddComponent(prefabEntity, entityFilter);
                }

                runOnce = false;
            }
        }

        public void SpawnEntities(int amountToSpawn, Entity objectToSpawn, Type entityFilter)
        {
            this.prefabEntity = objectToSpawn;
            this.amountToSpawn = amountToSpawn;
            runOnce = true;
            this.entityFilter = entityFilter;
            this.Update();
        }
    }
}