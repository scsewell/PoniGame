#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/*=============================================================================
 |	    Project:  Unity3D Scene OBJ Exporter
 |
 |		  Notes: Only works with meshes + meshRenderers. No terrain yet
 |
 |       Author:  aaro4130
 |
 |     DO NOT USE PARTS OF THIS CODE, OR THIS CODE AS A WHOLE AND CLAIM IT
 |     AS YOUR OWN WORK. USE OF CODE IS ALLOWED IF I (aaro4130) AM CREDITED
 |     FOR THE USED PARTS OF THE CODE.
 |
 *===========================================================================*/

public class UnityOBJExporter : ScriptableWizard {
	
	[MenuItem ("File/Export/Wavefront OBJ")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Export OBJ", typeof(UnityOBJExporter),"Export");
	}

	private string exportName = "UnityExport";
	private string exportFolder = "c:/UnityExporter/";
	public string objComment = "";
	public bool openExportDirWhenDone = false ;
	public bool exportTextures = true;
	public bool generateMaterials = true;
	public bool materialInstancePercautions = true;
	public bool matNameFromTextureName = false;
	public SplitType splitType;
	public bool selectedOnly = false;
	public bool applyScale = true;
	public bool applyPosition = true;
	public bool applyRotation = true;

		//public bool includeInactiveGameObjects = true;



	//public bool appendObjectNameToMaterialName = true;
	public enum SplitType{
		By_Mesh,
		By_Material,
		By_Submesh,
		None
	}
	void OnInspectorUpdate() {
		Repaint();

	}
	void OnWizardUpdate() {
		helpString = "Aaro4130's OBJ Exporter V1.05";
	}


	string ConstructFaceString(int i1, int i2, int i3){
		return "" + i1 + "/" + i2 + "/" + i3;
	}

	List<string> notReadableTextures = new List<string>();

	List<string> matNameCache = new List<string>();
	string GenerateMaterialNew(ref System.Text.StringBuilder sbR,ref Material m){


		string matName = m.name;
		if(matNameFromTextureName && m.HasProperty ("_MainTex")){
			Texture2D mT = m.GetTexture ("_MainTex") as Texture2D;
			if(mT != null){
				matName = m.GetTexture ("_MainTex").name;
			}else{
				Debug.Log ("Could not generate material from texture name (" + matName + "). No texture assigned");
			}
		}
		bool instance = matName.Contains ("Instance");
		if(instance && materialInstancePercautions) {
			
			matName += "_(" + m.GetInstanceID () + ")";
		}

		if(matNameCache.Contains (matName) == false){
			matNameCache.Add (matName);
		sbR.AppendLine ("newmtl " + matName);

			bool hasColor = m.HasProperty ("_Color");
			if(hasColor){
		Color matColor = m.color;
		sbR.AppendLine ("Kd " + matColor.r + " " + matColor.g + " " + matColor.b);
				float alpha = Mathf.Lerp (1,0,matColor.a);

				sbR.AppendLine ("d " + alpha);
			}

		bool hasSpecular = m.HasProperty ("_SpecColor");
		if (hasSpecular) {
			Color specColor = m.GetColor ("_SpecColor");
			sbR.AppendLine ("Ks " + specColor.r + " " + specColor.g + " " + specColor.b);
		}
		bool hasTexture = m.HasProperty ("_MainTex");
		if (hasTexture) {
				Texture2D mainTex = m.GetTexture ("_MainTex") as Texture2D;

				if(mainTex != null){
					Vector2 mainTexScale = m.GetTextureScale ("_MainTex");
					if(mainTex.wrapMode == TextureWrapMode.Clamp){
						sbR.AppendLine("-clamp on");
					}
					sbR.AppendLine ("s " + mainTexScale.x + " " + mainTexScale.y);

				sbR.AppendLine ("map_Kd " + mainTex.name + ".png");


					try{
						
						if(exportTextures  ){
							Texture2D nT = new Texture2D(mainTex.width,mainTex.height,TextureFormat.ARGB32,false);
							Color[] pxls = mainTex.GetPixels ();
							nT.SetPixels (pxls);
							
							byte[] pngEx = nT.EncodeToPNG ();
							
							if(System.IO.File.Exists (exportFolder + "/" + mainTex.name + ".png") == false){
								System.IO.File.WriteAllBytes (exportFolder + "/" + mainTex.name + ".png",pngEx);
							}
							
						}
					}catch(System.Exception ex){
	
					}
				}



		}
		sbR.AppendLine ();
		}
		return matName;
	}
	Vector3 RotateAroundPoint(Vector3 point,Vector3 pivot,Quaternion angle){
		return angle * (point - pivot) + pivot;
	}
	Vector3 MultiplyVec3s(Vector3 v1, Vector3 v2){
		return new Vector3(v1.x * v2.x,v1.y * v2.y,v1.z * v2.z);
	}


	void UpdateProgress(float prgrs,string mName){
		EditorUtility.DisplayProgressBar(
			"OBJ Exporter",
			"Exporting " + mName,
			prgrs);
	}

	void OnWizardCreate(){
		string expFile = EditorUtility.SaveFilePanel("Export OBJ","","unityexport.obj","obj");
		if(expFile.Length > 0){
			string fileName = System.IO.Path.GetFileNameWithoutExtension(expFile);
			exportName = fileName;
			exportFolder = System.IO.Path.GetDirectoryName (expFile);
			Export ();
		}
	}
	void Export(){
		EditorUtility.DisplayProgressBar(
			"OBJ Exporter",
			"Initializing...",
			0);

		EditorUtility.ClearProgressBar ();
		if(System.IO.Directory.Exists (exportFolder) == false){
			System.IO.Directory.CreateDirectory (exportFolder);
		}
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		System.Text.StringBuilder sb2 = new System.Text.StringBuilder();

		if(objComment.Length > 0)
			sb.AppendLine ("#" + objComment);


		if(generateMaterials)
			sb.AppendLine ("mtllib " + exportName + ".mtl");

		MeshFilter[] sceneMeshes;
		if(selectedOnly){
			List<MeshFilter> tempMFList = new List<MeshFilter>();
			foreach(GameObject g in Selection.gameObjects){

					MeshFilter f = g.GetComponent<MeshFilter>();
					if(f != null){
						tempMFList.Add (f);
					}
				
			}
			sceneMeshes = tempMFList.ToArray ();
		}else{
			sceneMeshes = FindObjectsOfType (typeof(MeshFilter)) as MeshFilter[];

		}

	
		int cFaceIndex = 1;

		if(splitType == SplitType.None){
			sb.AppendLine ("g default");
		}

		for (int i=0;i<sceneMeshes.Length;i++) {
			float cProgress = i / sceneMeshes.Length;
			UpdateProgress (cProgress,sceneMeshes[i].name);
							

			int wVerts = 0;
		
			GameObject pObj = sceneMeshes[i].gameObject;
			Mesh m = sceneMeshes[i].sharedMesh;
			if(splitType == SplitType.By_Mesh){
			sb.AppendLine ("g " + pObj.name + "[" + pObj.GetInstanceID () + "]");
			}


			foreach(Vector3 vrtx in m.vertices){
				Vector3 vx = vrtx;
				if(applyScale){
					vx = MultiplyVec3s(vx,pObj.transform.lossyScale);
				}
				if(applyRotation){
					vx = RotateAroundPoint (vx,Vector3.zero,pObj.transform.rotation);
				}
		
				if(applyPosition){
					vx += pObj.transform.position;
				}
				sb.AppendLine ("v " + (vx.x * -1) + " " + vx.y + " " + vx.z );
				wVerts += 1;



				
			}


			foreach(Vector3 vrtx in m.normals){
				Vector3 vx = vrtx;
				if(applyRotation){
					vx = RotateAroundPoint (vrtx,Vector3.zero,pObj.transform.rotation);
				}
				sb.AppendLine ("vn " + (vx.x * -1) + " " + vx.y + " " + vx.z );


			
		
			}

			foreach(Vector2 vx in m.uv){
				sb.AppendLine ("vt " + vx.x + " " + vx.y );
			}
			
			bool hasMeshRenderer = false;
			
			
			Renderer mr =  pObj.GetComponent<Renderer>();
			if(mr != null){
				hasMeshRenderer = true ;
			}
			
			for(int j  = 0; j < m.subMeshCount; j++){
				if(splitType == SplitType.By_Submesh){
					sb.AppendLine ("g " + pObj.name + "[" + pObj.GetInstanceID () + ",SM" + j + "]");
				}
				if(hasMeshRenderer && generateMaterials){
					if(j <= (mr.sharedMaterials.Length - 1)){
						Material ml  = mr.sharedMaterials[j];
						string matName = GenerateMaterialNew(ref sb2,ref ml);
						if(splitType == SplitType.By_Material){
							sb.AppendLine ("g " + matName);
						}
						sb.AppendLine("usemtl " + matName  );



						
						
					}
					
				}
				int[] indices = m.GetTriangles(j);
				
				for(int k = 0; k < indices.Length; k += 3){
					string face3 = ConstructFaceString(indices[k+2] + cFaceIndex,indices[k+2] + cFaceIndex,indices[k+2] + cFaceIndex);
					string face2 = ConstructFaceString(indices[k+1] + cFaceIndex,indices[k+1] + cFaceIndex,indices[k+1] + cFaceIndex);
					string face1 = ConstructFaceString(indices[k] + cFaceIndex,indices[k] + cFaceIndex,indices[k] + cFaceIndex);

					sb.AppendLine ("f " + face3 + " " + face2 + " " + face1);
				}
				
			}
			cFaceIndex += wVerts;
		}
		EditorUtility.DisplayProgressBar(
			"OBJ Exporter",
			"Writing OBJ file...",
			1);
		System.IO.File.WriteAllText (exportFolder + "/" + exportName + ".obj",sb.ToString ());
		System.IO.File.WriteAllText (exportFolder + "/" + exportName + ".mtl",sb2.ToString ());
		EditorUtility.ClearProgressBar ();
		if(openExportDirWhenDone){
			System.Diagnostics.Process.Start (exportFolder);
		}
	}
	
}
#endif 