// File from targeting system 2.0 that is a free-time project and thus is copyrighted to its owner.
// Copyright © 2020-2021 Mikael Korpinen(Finland). All Rights Reserved.
using FromTargetingSystem2._0.EntityComponents;
using Plugins.GeometricVision;
using Plugins.GeometricVision.EntityScripts.FromUnity;
using Plugins.GeometricVision.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

namespace FromTargetingSystem2._0
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [AlwaysUpdateSystem]
    public class MissileSystem : SystemBase
    {
        private EntityCommandBuffer ecb;
        EntityCommandBufferSystem endSimulationEcbSystem;

        private EntityQuery m_Group = new EntityQuery();
        private GeometryVision geoVision;
        private bool spawnMissile;
        private float3 spawnPosition;
        private Quaternion spawnRotation;
        private GeometryDataModels.Target closestTarget;
        private float3 speed;
        private float3 rotationSpeed;
        private Entity explosion;
        private float4 explosionParticleDuration = 2;
        private DynamicBuffer<LinkedEntityGroup> childrenOfRemains;
        private DynamicBuffer<LinkedEntityGroup> childrenOfExplosion;
        private bool killAllEntities;
        public GeometryVision GeoVision
        {
            get { return geoVision; }
            set { geoVision = value; }
        }

        public Entity Explosion
        {
            get { return explosion; }
            set { explosion = value; }
        }

        public Entity EnemyRemains { get; set; }

        public bool KillAllEntities
        {
            get { return killAllEntities; }
            set { killAllEntities = value; }
        }

        protected override void OnCreate()
        {
            childrenOfRemains = new DynamicBuffer<LinkedEntityGroup>();
            childrenOfExplosion = new DynamicBuffer<LinkedEntityGroup>();
            // Find the ECB system once and store it for later usage
            endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        /// <summary>
        /// Spawns missile and guides towards its target. Also
        /// </summary>
        protected override void OnUpdate()
        {
            var commandBuffer = endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var remainsIn = this.EnemyRemains;
            var explosionIn = this.explosion;
            var deltaTime = Time.DeltaTime;
            var speedIn = speed;
            var rotationSpeedIn = rotationSpeed;
        
            if (EntityManager.Exists(this.EnemyRemains) == false)
            {
                return;
            }

            if (killAllEntities)
            {
                Entities.WithAll<RotationSpeed_SpawnAndRemove>()
                    .ForEach((Entity entity, 
                        int entityInQueryIndex) =>
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                    })
                    .ScheduleParallel(this.Dependency).Complete();
                killAllEntities = false;
            }
        
            var childrenOfRemainsIn = childrenOfRemains;
            var childrenOfExplosionIn = childrenOfExplosion;
            childrenOfRemainsIn = HandleChildData(remainsIn);

            if (spawnMissile && geoVision != null)
            {
                closestTarget = geoVision.GetClosestTarget();
                childrenOfExplosionIn = HandleChildData(explosionIn);
                AdjustChilds(childrenOfExplosionIn);

                void AdjustChilds(
                    DynamicBuffer<LinkedEntityGroup> linkedEntityGroups)
                {
        
                    foreach (var entityGroup in linkedEntityGroups)
                    {
                        
                        if (HasComponent<MarkedForDestruction>(entityGroup.Value))
                        {
                            
                            SetComponent(entityGroup.Value, new MarkedForDestruction()
                            {
                                AmountOfLifeTimeLeft = new float4(explosionParticleDuration),
                                TakeTargetDownRadius = 0,
                            });
                        }
                    }
                }

                HandleSpawning(commandBuffer, closestTarget);
                spawnMissile = false;
            }
        
            //Move missile
            //This cannot be refactored in to local function.(Reason) Should be replaced by code gen error.
            Entities
                .ForEach((Entity entity, int entityInQueryIndex, ref MissileMovement missileMovement,
                    in MissileTargetLock targetLockComponent,
                    in MissileTimer timer,
                    in Translation position, in Rotation rotation) =>
                {
                    HandleMissilesMovementsAndRotations(entity, position,ref missileMovement, rotation, entityInQueryIndex,
                        timer, targetLockComponent);
                }).Schedule();

            HandlePhysicsFromMissileExplosion(commandBuffer, deltaTime);

            Entities.WithoutBurst().WithAll<MarkedForDestruction>()
                .ForEach((Entity entity, 
                    int entityInQueryIndex,
                    UnityEngine.ParticleSystem ps,
                    in MarkedForDestruction markedForDestruction) =>
                {
                    if (markedForDestruction.AmountOfLifeTimeLeft.x < 0.1)
                    {
                        ps.Stop();
                    }
                })
                .Run();

            Entities.ForEach((Entity entity, int entityInQueryIndex, ref MarkedForDestruction markedForDestruction) =>
            {
                if (markedForDestruction.AmountOfLifeTimeLeft.x > 0)
                {
                    markedForDestruction.AmountOfLifeTimeLeft.x -= deltaTime;
                }
            }).WithBurst().ScheduleParallel();
            endSimulationEcbSystem.AddJobHandleForProducer(Dependency);

            //
            //Local functions
            //

            DynamicBuffer<LinkedEntityGroup> HandleChildData(Entity enitityIn)
            {
                ecb = new EntityCommandBuffer(Allocator.TempJob);
                var children = EntityManager.GetBuffer<LinkedEntityGroup>(enitityIn);
                ecb.AddBuffer<Child>(enitityIn);
                ecb.Dispose();
                return children;
            }

            void HandleMissilesMovementsAndRotations(Entity entity, Translation position,
                ref MissileMovement missileMovement,
                Rotation rotation, int entityInQueryIndex, MissileTimer timer, MissileTargetLock lockedTarget)
            {
                var newPos = HandleMissileMovementAndRotation(position, ref missileMovement, rotation,
                    entityInQueryIndex, entity, lockedTarget);

                HandLeMissileCloseToTarget(position, lockedTarget, entityInQueryIndex, entity, newPos);
            }


            float3 HandleMissileMovementAndRotation(Translation position, ref MissileMovement missileMovement,
                Rotation rotation,
                int entityInQueryIndex, Entity entity, MissileTargetLock missileTargetLock)
            {


                if (missileMovement.CurrentSpeed.x < missileMovement.MaxSpeed.x)
                {
                    missileMovement.CurrentSpeed += speedIn* deltaTime  * missileMovement.CurrentSpeed  *  missileMovement.CurrentSpeed ;
                        
                }
                missileMovement.RotationSpeed += missileMovement.RotationSpeed * missileMovement.RotationSpeed * missileMovement.RotationSpeed * rotationSpeedIn * deltaTime;
                var rot = GeometryVisionUtilities.RotateTowardsTarget(position.Value, missileTargetLock.Target,
                    missileMovement.RotationSpeed.x, rotation.Value);

                commandBuffer.AddComponent<Rotation>(entityInQueryIndex, entity);
                commandBuffer.SetComponent(entityInQueryIndex, entity, new Rotation()
                {
                    Value = rot
                });
                    

                var newPos = GeometryVisionUtilities.MoveTowards(position.Value,
                    position.Value + (float3) (rot * Vector3.forward), missileMovement.CurrentSpeed.x/100 );
                commandBuffer.AddComponent<Translation>(entityInQueryIndex, entity);
                commandBuffer.SetComponent(entityInQueryIndex, entity, new Translation
                {
                    Value = newPos
                });
                return newPos;
            }

            void HandLeMissileCloseToTarget(Translation position, MissileTargetLock lockedTarget,
                int entityInQueryIndex, Entity entity, float3 newPosition)
            {
                if (HasComponent<Translation>(entity)  && GeometryVisionUtilities.Float3Distance(position.Value, lockedTarget.Target.position).x < 2)
                {

                    //Destroy the target
                    commandBuffer.DestroyEntity(entityInQueryIndex, lockedTarget.Target.entity);
                
                    //explosion in
                    //
                    //
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                    SetDataForExplosion(commandBuffer, entityInQueryIndex, explosionIn, newPosition, lockedTarget, Vector3.zero, new bool4(false));
                    commandBuffer.Instantiate(entityInQueryIndex, explosionIn);
                    //Remains in
                    //
                    //

                    SetDataForExplosion(commandBuffer, entityInQueryIndex, remainsIn, lockedTarget.Target.position,
                        lockedTarget, lockedTarget.Target.position - newPosition,
                        new bool4(true));

                    AdjustChilds(entityInQueryIndex, lockedTarget, lockedTarget.Target.position, childrenOfRemainsIn, new bool4(true));

                    commandBuffer.Instantiate(entityInQueryIndex, remainsIn);

                    void AdjustChilds(int entityInQueryIndexIn, MissileTargetLock lockedTargetIn, float3 newPos,
                        DynamicBuffer<LinkedEntityGroup> linkedEntityGroups, bool4 addPhysics)
                    {
                        for (int i = 0; i < linkedEntityGroups.Length; i++)
                        {
                            SetDataForExplosion(commandBuffer, entityInQueryIndexIn, linkedEntityGroups[i].Value,
                                lockedTargetIn.Target.position +
                                GetComponent<LocalToWorld>(linkedEntityGroups[i].Value).Position, lockedTargetIn,
                                lockedTargetIn.Target.position - newPos, addPhysics);
                        }
                    }
                }

                void SetDataForExplosion(EntityCommandBuffer.ParallelWriter parallelWriter, int entityInQueryIndexIn,
                    Entity entityToSetDataFor, float3 newPos,
                    MissileTargetLock lockedTarget, float3 forceDirection, bool4 addPhysics)
                {
                    if (HasComponent<Translation>(entityToSetDataFor))
                    {
                        parallelWriter.SetComponent(entityInQueryIndexIn, entityToSetDataFor, new Translation
                        {
                            Value = newPos
                        });
                    }

                    if (addPhysics.x)
                    {
                        parallelWriter.AddComponent(entityInQueryIndexIn, entityToSetDataFor, new Explosion
                        {
                            force = 10,
                            direction = forceDirection,
                            Duration = 2.5f
                        });
                    }

                    parallelWriter.AddComponent(entityInQueryIndexIn, entityToSetDataFor,
                        new MissileTargetLock()
                        {
                            Target = lockedTarget.Target
                        });
                    parallelWriter.AddComponent<MarkedForDestruction>(entityInQueryIndexIn, entityToSetDataFor);
                    parallelWriter.SetComponent(entityInQueryIndexIn, entityToSetDataFor, new MarkedForDestruction()
                    {
                        AmountOfLifeTimeLeft = 2,
                    });
                }
            }
        }

        void HandlePhysicsFromMissileExplosion(EntityCommandBuffer.ParallelWriter commandBuffer, float deltaTime)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entities
                //  .WithReadOnly(target)
                .ForEach((Entity entity, int entityInQueryIndex, ref PhysicsVelocity physicsVelocity,
                    ref PhysicsMass physicsMass, ref Explosion explosionComponent,
                    in MissileTargetLock missileTargetLock) =>
                {
                    explosionComponent.Duration -= deltaTime;
                    if (explosionComponent.Exploded.x == false)
                    {
                        PhysicsComponentExtensions.ApplyLinearImpulse(ref physicsVelocity, physicsMass,
                            explosionComponent.direction * explosionComponent.force);
                        explosionComponent.Exploded.x = true;
                    }

                    if ((explosionComponent.Duration < 0.1f).x)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                        if (missileTargetLock.Target.entity.Equals(Entity.Null) == false)
                        {
                            commandBuffer.DestroyEntity(missileTargetLock.Target.entity.Index,
                                missileTargetLock.Target.entity);
                        }
                    }
                }).WithoutBurst().Schedule();
        }

        private void HandleSpawning(EntityCommandBuffer.ParallelWriter commandBuffer, GeometryDataModels.Target targetIn)
        {
            float3 spawnPosition = this.spawnPosition;
            var rotation = this.spawnRotation;
            var speedIn = this.speed;
            var rotationSpeedIn = this.rotationSpeed;
            var lockedTarget = targetIn;

            Entities.WithNone<MissileTargetLock>()
                .WithStoreEntityQueryInField(ref m_Group)
                .ForEach(
                    (Entity entity, int entityInQueryIndex, in MissileMovement_SpawnData spawner,
                        in LocalToWorld location) =>
                    {
                        SpawnAndHandleComponents(spawner, commandBuffer, entityInQueryIndex, spawnPosition, rotation,
                            speedIn, rotationSpeedIn, lockedTarget);
                    }).ScheduleParallel();
        }

        private static void SpawnAndHandleComponents(MissileMovement_SpawnData spawner,
            EntityCommandBuffer.ParallelWriter commandBuffer,
            int entityInQueryIndex, float3 spawnPos, Quaternion rotation, float3 speed, float3 rotationSpeedIn,
            GeometryDataModels.Target lockedTarget)
        {
            var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.Prefab);

            commandBuffer.SetComponent(entityInQueryIndex, instance,
                new Translation {Value = spawnPos});

            commandBuffer.SetComponent(entityInQueryIndex, instance,
                new Rotation {Value = rotation});
            commandBuffer.AddComponent(entityInQueryIndex, instance,
                new MissileTimer() {Value = 4,});
            commandBuffer.AddComponent(entityInQueryIndex, instance,
                new MissileMovement
                {
                    Speed = speed,
                    MaxSpeed = spawner.MaxSpeed,
                    CurrentSpeed = speed,
                    RotationSpeed = rotationSpeedIn,
                });
            commandBuffer.AddComponent(entityInQueryIndex, instance, new MissileTargetLock()
            {
                Target = lockedTarget
            });
        }

        public void spawnMissileAndUpdate(GeometryVision geometryVision, float3 spawnPosition,
            Quaternion spawnDirection, float3 speedIn, float3 rotationSpeedIn)
        {
            this.geoVision = geometryVision;
            spawnMissile = true;
            this.spawnPosition = spawnPosition;
            this.spawnRotation = spawnDirection;
            this.speed = speedIn;
            this.rotationSpeed = rotationSpeedIn;

            this.Update();
        }
    
    
    }
}