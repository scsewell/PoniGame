using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    /*
     * Gets the bearing in degrees between two vectors projected onto a plane
     */
    public static float GetBearing(Vector3 vec1, Vector3 vec2, Vector3 planeNormal)
    {
        Vector3 dir1 = Vector3.ProjectOnPlane(vec1, planeNormal).normalized;
        Vector3 dir2 = Vector3.ProjectOnPlane(vec2, planeNormal).normalized;
        return Mathf.Sign(Vector3.Dot(Vector3.Cross(dir1, dir2), planeNormal)) * Vector3.Angle(dir1, dir2);
    }
}
