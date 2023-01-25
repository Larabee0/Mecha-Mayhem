using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class ExtraMaths
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ProjectOnPlane(float3 vector, float3 planeNormal)
    {
        float num = math.dot(planeNormal, planeNormal);
        if (num < math.EPSILON)
            return vector;
        float num2 = math.dot(vector, planeNormal);
        return new float3(vector.x - planeNormal.x * num2 / num, vector.y - planeNormal.y * num2 / num, vector.z - planeNormal.z * num2 / num);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SqrMagnitude(float3 vector) { return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(float3 from, float3 to)
    {
        // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
        float denominator = math.sqrt(SqrMagnitude(from) * SqrMagnitude(to));
        if (denominator < 1E-15f)
            return 0f;

        float dot = math.clamp(math.dot(from, to) / denominator, -1f, 1f);
        return math.acos(dot) * 57.29578f;
    }

    /// <summary>
    /// Gets the world position where a ray intersects a given point in the world Y axis.
    /// </summary>
    /// <param name="ray">Intersection Ray</param>
    /// <param name="height">World Y axis Point</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetPointAtHeight(Ray ray, float height = 0f)
    {
        float dst = GetDstToCrossOverHeight(ray, height);
        float3 end = ray.GetPoint(dst);
        end.y = height;
        return end;
    }

    /// <summary>
    /// Gets the distance along a ray where it intersects a given point in the world Y axis.
    /// </summary>
    /// <param name="ray">Intersection Ray</param>
    /// <param name="height">World Y axis Point</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetDstToCrossOverHeight(Ray ray, float height = 0f)
    {
        float dst = (ray.origin.y - height) / math.sin(math.radians(Angle(ray.direction, ProjectOnPlane(ray.direction, math.up()))));
        if (float.IsNaN(dst) || !float.IsFinite(dst) || float.IsInfinity(dst))
        {
            dst = 0f;
        }
        return dst;
    }
}
