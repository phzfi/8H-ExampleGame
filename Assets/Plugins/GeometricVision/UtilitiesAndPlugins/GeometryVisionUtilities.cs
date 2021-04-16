using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.GeometricVision.EntityScripts;
using Plugins.GeometricVision.ImplementationsEntities;
using Plugins.GeometricVision.ImplementationsGameObjects;
using Plugins.GeometricVision.Interfaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Plugins.GeometricVision.Utilities
{
    public static class GeometryVisionUtilities
    {
        /// <summary>
        /// Moves transform. GameObject version of the move target
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="newPosition"></param>
        /// <param name="movementSpeed"></param>
        /// <param name="distanceToStop"></param>
        public static IEnumerator MoveTarget(Transform targetTransform, Vector3 newPosition, float movementSpeed,
            float distanceToStop)
        {
            float timeOut = 10f;

            while (targetTransform && Vector3.Distance(targetTransform.position, newPosition) > distanceToStop)
            {
                var animatedPoint = Vector3.MoveTowards(targetTransform.position, newPosition, movementSpeed);
                targetTransform.position = animatedPoint;
                if (timeOut < 0.05f)
                {
                    break;
                }

                timeOut -= Time.deltaTime;

                yield return null;
            }
        }
        
        /// <summary>
        ///   <para>Returns the distance between a and b.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        [BurstCompile]
        public static float4 Float4Distance(float4 a, float4 b)
        {
            float num1 = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float4) Math.Sqrt(num1 * num1 + num2 * num2 + num3 * num3);
        }

        /// <summary>
        ///   <para>Returns the distance between a and b.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        [BurstCompile]
        public static float3 Float3Distance(float3 a, float3 b)
        {
            float num1 = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float3) Math.Sqrt(num1 * num1 + num2 * num2 + num3 * num3);
        }

        public static Quaternion RotateTowardsTarget(float3 spawnPosition, GeometryDataModels.Target target, float rotateStepAmount,
            Quaternion rotationTochange)
        {
            float3 newDirection = GetOffsetToOriginByPosition(target, spawnPosition);
            rotationTochange = Quaternion.RotateTowards(rotationTochange,
                Quaternion.LookRotation(newDirection, Vector3.up), rotateStepAmount);

            float3 GetOffsetToOriginByPosition(GeometryDataModels.Target targetToOffset, float3 locationToUseAsOffset)
            {
                return targetToOffset.position - locationToUseAsOffset;
            }

            return rotationTochange;
        }
        
        /// <summary>
        ///   <para>Calculate a position between the points specified by current and target, moving no farther than the distance specified by maxDistanceDelta.</para>
        /// </summary>
        /// <param name="current">The position to move from.</param>
        /// <param name="target">The position to move towards.</param>
        /// <param name="maxDistanceDelta">Distance to move current per call.</param>
        /// <returns>
        ///   <para>The new position.</para>
        /// </returns>
        public static float3 MoveTowards(
            float3 current,
            float3 target,
            float maxDistanceDelta)
        {
            float num1 = target.x - current.x;
            float num2 = target.y - current.y;
            float num3 = target.z - current.z;
            float num4 = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 +
                                  (double) num3 * (double) num3);
            if ((double) num4 == 0.0 || (double) maxDistanceDelta >= 0.0 &&
                (double) num4 <= (double) maxDistanceDelta * (double) maxDistanceDelta)
                return target;
            float num5 = (float) Math.Sqrt((double) num4);
            return new float3(current.x + num1 / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta,
                current.z + num3 / num5 * maxDistanceDelta);
        }

        internal static IEnumerator MoveEntityTarget(Vector3 newPosition,
            float speedMultiplier, GeometryDataModels.Target target, float distanceToStop,
            TransformEntitySystem transformEntitySystem, World entityWorld,
            NativeList<GeometryDataModels.Target> closestTargets)
        {
            if (transformEntitySystem == null)
            {
                transformEntitySystem = entityWorld.CreateSystem<TransformEntitySystem>();
            }

            float timeOut = 2f;
            while (Vector3.Distance(target.position, newPosition) > distanceToStop)
            {
                var animatedPoint =
                    transformEntitySystem.MoveEntityToPosition(newPosition, target, speedMultiplier);

                target.position = animatedPoint;
                if (closestTargets.Length != 0)
                {
                    closestTargets[0] = target;
                }

                if (timeOut < 0.1f)
                {
                    break;
                }

                timeOut -= Time.deltaTime;

                yield return null;
            }
        }

        public static bool TargetHasNotChanged(GeometryDataModels.Target newTarget,
            GeometryDataModels.Target currentTarget)
        {
            return newTarget.GeoInfoHashCode == currentTarget.GeoInfoHashCode
                   && newTarget.entity.Index == currentTarget.entity.Index
                   && newTarget.entity.Version == currentTarget.entity.Version;
        }

        //Usage: this.targets.Sort<GeometryDataModels.Target, DistanceComparer>(new DistanceComparer());
        public struct DistanceComparerToViewDirection : IComparer<GeometryDataModels.Target>
        {
            public int Compare(GeometryDataModels.Target x, GeometryDataModels.Target y)
            {
                if (x.distanceToCastOrigin == 0f && y.distanceToCastOrigin != 0f)
                {
                    return 1;
                }

                if (x.distanceToCastOrigin != 0f && y.distanceToCastOrigin == 0f)
                {
                    return -1;
                }

                if (x.distanceToRay < y.distanceToRay)
                {
                    return -1;
                }

                else if (x.distanceToRay > y.distanceToRay)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        //Usage: this.targets.Sort<GeometryDataModels.Target, DistanceComparer>(new DistanceComparer());
        public struct DistanceComparerToCamera : IComparer<GeometryDataModels.Target>
        {
            public int Compare(GeometryDataModels.Target x, GeometryDataModels.Target y)
            {
                if (x.distanceToCastOrigin == 0f && y.distanceToCastOrigin != 0f)
                {
                    return 1;
                }

                if (x.distanceToCastOrigin != 0f && y.distanceToCastOrigin == 0f)
                {
                    return -1;
                }

                if (x.distanceToCastOrigin < y.distanceToCastOrigin)
                {
                    return -1;
                }

                else if (x.distanceToCastOrigin > y.distanceToCastOrigin)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        internal static bool TransformIsEffect(string nameOfTransform)
        {
            if (nameOfTransform.Length < GeometryVisionSettings.NameOfEndEffect.Length
                && nameOfTransform.Length < GeometryVisionSettings.NameOfMainEffect.Length
                && nameOfTransform.Length < GeometryVisionSettings.NameOfStartingEffect.Length)
            {
                return false;
            }

            return nameOfTransform.Contains(GeometryVisionSettings.NameOfStartingEffect) ||
                   nameOfTransform.Contains(GeometryVisionSettings.NameOfMainEffect) ||
                   nameOfTransform.Contains(GeometryVisionSettings.NameOfEndEffect);
        }


        public static void HandleEntityImplementationAddition<TImplementation, TCollection>(
            TImplementation entitySystemToAdd, HashSet<TCollection> listOfInterfaces, World eWorld, Action action)
            where TImplementation : ComponentSystemBase, TCollection, new()
        {
            if (entitySystemToAdd == null)
            {
                if (eWorld == null)
                {
                    eWorld = World.DefaultGameObjectInjectionWorld;
                }

                var eye = eWorld.CreateSystem<TImplementation>();
                InterfaceUtilities.AddImplementation(eye, listOfInterfaces);
                action();
            }
        }

        /// <summary>
        /// Clears up current targeting programs and creates a new, then proceeds to add all the available targeting systems
        /// to the targeting systems container
        /// </summary>
        internal static void UpdateTargetingSystemsContainer(List<TargetingInstruction> targetingInstructions,
            GeometryTargetingSystemsContainer targetingSystemsContainer)
        {
            targetingSystemsContainer.TargetingPrograms = new HashSet<IGeoTargeting>();

            foreach (var targetingInstruction in targetingInstructions)
            {
                if (targetingInstruction.TargetingSystemGameObjects != null)
                {
                    targetingSystemsContainer.AddTargetingProgram(targetingInstruction.TargetingSystemGameObjects);
                }

                if (targetingInstruction.TargetingSystemEntities != null)
                {
                    targetingSystemsContainer.AddTargetingProgram(targetingInstruction.TargetingSystemEntities);
                }
            }
        }

        /// <summary>
        /// In case the user plays around with the settings on the inspector and changes thins this needs to be run.
        /// It checks that the targeting system implementations are correct.
        /// </summary>
        /// <param name="targetingInstructionsIn"></param>
        /// <param name="gameObjectProcessing"></param>
        /// <param name="entityBasedProcessing"></param>
        /// <param name="entityWorld"></param>
        public static List<TargetingInstruction> ValidateTargetingSystems(
            List<TargetingInstruction> targetingInstructionsIn, bool gameObjectProcessing, bool entityBasedProcessing,
            World entityWorld)
        {
            ValidatePresentTargetingInstructions();

            void ValidatePresentTargetingInstructions()
            {
                foreach (var targetingInstruction in targetingInstructionsIn)
                {
                    if (gameObjectProcessing == true)
                    {
                        targetingInstruction.TargetingSystemGameObjects =
                            AssignNewTargetingSystemToTargetingInstruction(targetingInstruction,
                                new GeometryObjectTargeting(), new GeometryLineTargeting());
                    }

                    if (entityBasedProcessing == true && Application.isPlaying)
                    {
                        if (entityWorld == null)
                        {
                            entityWorld = World.DefaultGameObjectInjectionWorld;
                        }

                        var newObjectTargeting = entityWorld.CreateSystem<GeometryEntitiesObjectTargeting>();
                        var newLineTargeting = entityWorld.CreateSystem<GeometryEntitiesLineTargeting>();

                        targetingInstruction.TargetingSystemEntities =
                            AssignNewTargetingSystemToTargetingInstruction(targetingInstruction, newObjectTargeting,
                                newLineTargeting);
                    }
                }
            }

            return targetingInstructionsIn;

            // Local functions

            IGeoTargeting AssignNewTargetingSystemToTargetingInstruction(TargetingInstruction targetingInstruction,
                IGeoTargeting newObjectTargeting,
                IGeoTargeting newLineTargeting)
            {
                IGeoTargeting targetingToReturn = null;

                if (targetingInstruction.GeometryType == GeometryType.Objects)
                {
                    targetingToReturn = newObjectTargeting;
                }

                if (targetingInstruction.GeometryType == GeometryType.Lines)
                {
                    targetingToReturn = newLineTargeting;
                }

                return targetingToReturn;
            }
        }
    }
}