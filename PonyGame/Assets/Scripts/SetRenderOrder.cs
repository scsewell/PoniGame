using UnityEngine;
using System.Collections;

public class SetRenderOrder : MonoBehaviour 
{
	// Sets the render order for a gameobject explicitly.
	// This avoids Outlined objects switching which one renders first causing rendering artifacts
	// -1 means use default render que from shader
	// Should be between 3000 and 4000
	[Range(3100, 3900)]
	public int renderQue = -1;

	// Use this for initialization
	void Start () 
	{
		if (GetComponent<MeshRenderer>() != null)
		{
			foreach (Material mat in GetComponent<MeshRenderer>().materials)
			{
				mat.renderQueue = renderQue;
			}
		}
		
		if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			foreach (Material mat in GetComponent<SkinnedMeshRenderer>().materials)
			{
				mat.renderQueue = renderQue;
			}
		}
	}
}
