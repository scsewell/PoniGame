using UnityEngine;
using System.Collections;

public class InterpolatorUpdater : MonoBehaviour
{
	void FixedUpdate()
    {
        foreach (TransformInterpolator interpolator in GetComponents<TransformInterpolator>())
        {
            interpolator.LateFixedUpdate();
        }
        foreach (FloatInterpolator interpolator in GetComponents<FloatInterpolator>())
        {
            interpolator.LateFixedUpdate();
        }
    }
}
