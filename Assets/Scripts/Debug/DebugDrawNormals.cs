using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawNormals : MonoBehaviour
{
    [SerializeField, Min(0)] float length = 1f;

    Mesh mesh;
    MeshFilter MeshFilter => GetComponent<MeshFilter>();
    MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    int i;
    int randomIndex;

    private void OnDrawGizmosSelected()
    {
        mesh = MeshFilter.mesh;
        Debug.Log(mesh.vertices.Length);

        for (i = 0; i < mesh.vertices.Length && i < 100; i++)
        {
            randomIndex = (int) (Random.Range(0f, 1f) * mesh.vertices.Length);
            Gizmos.DrawLine(
                transform.TransformPoint(mesh.vertices[randomIndex]), 
                transform.TransformPoint(mesh.vertices[randomIndex]) + mesh.normals[randomIndex] * length);
        }
    }
}
