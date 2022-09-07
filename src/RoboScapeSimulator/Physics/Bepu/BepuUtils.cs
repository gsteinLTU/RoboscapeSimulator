using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuUtilities.Memory;
using RoboScapeSimulator.Entities;

namespace RoboScapeSimulator.Physics.Bepu
{
    /// <summary>
    /// Various utility and helper functions
    /// </summary>
    public static class BepuUtils
    {
        public unsafe static bool QuickRayCast(Simulation simulation, Vector3 origin, Vector3 direction, float maxRange = 300, IEnumerable<DynamicEntity>? ignoreList = null)
        {
            int intersectionCount = 0;
            simulation.BufferPool.Take(1, out Buffer<RayHit> results);

            HitHandler hitHandler = new(results, &intersectionCount, ignoreList);

            simulation.RayCast(origin, direction, maxRange / 100f, ref hitHandler);
            simulation.BufferPool.Return(ref results);
            return intersectionCount > 0;
        }
    }

    public struct RayHit
    {
        public Vector3 Normal;
        public float T;
        public CollidableReference Collidable;
        public bool Hit;
    }

    public unsafe struct HitHandler : IRayHitHandler
    {
        public HitHandler(Buffer<RayHit> hits, int* intersectionCount, List<BodyHandle>? ignoreList)
        {
            Hits = hits;
            IntersectionCount = intersectionCount;
            IgnoreList = ignoreList;
        }

        public HitHandler(Buffer<RayHit> hits, int* intersectionCount, IEnumerable<DynamicEntity>? ignoreList = null)
        {
            Hits = hits;
            IntersectionCount = intersectionCount;
            IgnoreList = ignoreList?.Select(e => (e.BodyReference as SimBodyBepu).BodyReference.Handle).ToList();
        }

        public List<BodyHandle>? IgnoreList = null;
        public Buffer<RayHit> Hits;
        public int* IntersectionCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            if (IgnoreList != null)
            {
                return !IgnoreList.Contains(collidable.BodyHandle);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            if (IgnoreList != null)
            {
                return !IgnoreList.Contains(collidable.BodyHandle);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
        {
            maximumT = t;
            ref var hit = ref Hits[ray.Id];
            // if (t < hit.T)
            // {
            //     if (hit.T != float.MaxValue)
            ++*IntersectionCount;
            hit.Normal = normal;
            hit.T = t;
            hit.Collidable = collidable;
            hit.Hit = true;
            // }
        }
    }
}