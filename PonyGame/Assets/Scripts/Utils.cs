using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    public static Vector3 SwizzleXZY(Vector3 unityCoords)
    {
        return new Vector3(unityCoords.x, unityCoords.z, unityCoords.y);
    }

    public static Vector2 Rotate(Vector2 v, float radians)
    {
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;

        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }

    /*
     * Gets the bearing in degrees between two vectors projected onto a plane
     */
    public static float GetBearing(Vector3 vec1, Vector3 vec2, Vector3 planeNormal)
    {
        Vector3 dir1 = Vector3.ProjectOnPlane(vec1, planeNormal).normalized;
        Vector3 dir2 = Vector3.ProjectOnPlane(vec2, planeNormal).normalized;
        return Mathf.Sign(Vector3.Dot(Vector3.Cross(dir1, dir2), planeNormal)) * Vector3.Angle(dir1, dir2);
    }

    public static Vector2 CartisianToEquirectangular(Vector3 vec)
    {
        return new Vector2(-Mathf.Atan2(vec.y, vec.x), Mathf.PI - Mathf.Acos(vec.z / vec.magnitude));
    }
}
