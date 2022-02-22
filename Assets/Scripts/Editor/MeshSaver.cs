/**
 * File Name: MeshSaver.cs
 * Description: Saves a mesh after right clicking on the MeshFilter context in the Unity Editor
 * 
 * Authors: Pharan, Will Lacey
 * Date Created: September 2, 2021
 * 
 * Additional Comments: 
 *		The original version of this file can be found here:
 *      https://answers.unity.com/questions/39311/editor-scripting-how-to-save-a-script-generated-me.html within the
 *      comment section; this file has been updated it to better fit this project
 * 
 *      File Line Length: 120
 **/

using UnityEngine;
using UnityEditor;

public static class MeshSaver
{
	[MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
	public static void SaveMeshInPlace(MenuCommand menuCommand)
	{
		MeshFilter meshFilter = menuCommand.context as MeshFilter;
		Mesh mesh = meshFilter.sharedMesh;
		SaveMesh(mesh, mesh.name, false, true);
	}

	[MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
	public static void SaveMeshNewInstanceItem(MenuCommand menuCommand)
	{
		MeshFilter meshFilter = menuCommand.context as MeshFilter;
		Mesh mesh = meshFilter.sharedMesh;
		SaveMesh(mesh, mesh.name, true, true);
	}

	public static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
	{
		string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
		if (string.IsNullOrEmpty(path)) return;

		path = FileUtil.GetProjectRelativePath(path);

		Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

		if (optimizeMesh) MeshUtility.Optimize(meshToSave);

		AssetDatabase.CreateAsset(meshToSave, path);
		AssetDatabase.SaveAssets();
	}
}