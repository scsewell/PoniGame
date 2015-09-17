using UnityEngine;
using System.Collections;

public class ColorRandomizer : MonoBehaviour
{
	void Start ()
    {
	    if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().material.color = new Color(Random.Range(0.25f, 0.95f), Random.Range(0.25f, 0.95f), Random.Range(0.25f, 0.95f));
        }
	}
}
