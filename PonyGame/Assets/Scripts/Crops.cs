using UnityEngine;
using System.Collections;

public class Crops : MonoBehaviour
{
    public Transform[] crops;

    // Use this for initialization
    void Start()
    {
        Transform crop = Instantiate(crops[Random.Range(0, crops.Length)], transform.position, Quaternion.identity) as Transform;
        crop.SetParent(transform, true);
	}
}
