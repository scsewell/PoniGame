using UnityEngine;
using System.Collections;

public class Utils
{
    /*
     * Gets the bearing in degrees between two vectors as viewed from above
     */
    public static float GetBearing(Vector3 dir1, Vector3 dir2)
    {
        float vec1 = Quaternion.LookRotation(Vector3.ProjectOnPlane(dir1, Vector3.up), Vector3.up).eulerAngles.y;
        float vec2 = Quaternion.LookRotation(Vector3.ProjectOnPlane(dir2, Vector3.up), Vector3.up).eulerAngles.y;
        return -Mathf.DeltaAngle(vec2, vec1);
    }


    /*
     * Samples the ground beneath the transform in a circle and returns the average normal of the ground at those points
     */
    public static Vector3 GetGroundNormal(Transform t, CharacterController collider, int samples, float radius, bool drawDebug)
    {
        Vector3 normal = Vector3.zero;

        for (int i = 0; i < samples; i++)
        {
            Vector3 offset = Quaternion.Euler(0, i * (360.0f / samples), 0) * Vector3.forward * radius;
            Vector3 SamplePos = t.TransformPoint(offset + collider.center);
            Vector3 SampleDir = t.TransformPoint(offset + Vector3.down * 0.05f);

            RaycastHit hit;
            if (Physics.Linecast(SamplePos, SampleDir, out hit) && hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground") && Vector3.Dot(hit.normal, Vector3.up) > 0.75f)
            {
                normal += hit.normal;

                if (drawDebug)
                {
                    Debug.DrawLine(SamplePos, hit.point, Color.cyan);
                    Debug.DrawLine(hit.point, hit.point + hit.normal * 0.25f, Color.yellow);
                }
            }
        }

        if (normal != Vector3.zero)
        {
            if (drawDebug)
            {
                Debug.DrawLine(t.position, t.position + normal.normalized * 0.35f, Color.red);
            }

            return normal.normalized;
        }
        else
        {
            return Vector3.up;
        }
    }
}
