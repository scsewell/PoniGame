using UnityEngine;
using System.Collections;

public class ShapeKeys : MonoBehaviour
{
	public Transform animatedBone;
    public int shapeKeyIndex = 0;

	// Update is called once per frame
	void Update ()
    {
		if (animatedBone != null)
        {
            float value = (animatedBone.localPosition.x - 0.5f) * 50.0f;
            GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight (shapeKeyIndex, value);
		}
	}
}