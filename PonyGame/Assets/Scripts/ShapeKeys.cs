using UnityEngine;
using System.Collections;

public class ShapeKeys : MonoBehaviour
{
	public Transform animatedBone;
    public int shapeKeyIndex = 0;
    
	void LateUpdate()
    {
		if (animatedBone != null)
        {
            float value = (animatedBone.localPosition.x - 0.5f) * 50.0f;
            GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight (shapeKeyIndex, value);
		}
	}
}