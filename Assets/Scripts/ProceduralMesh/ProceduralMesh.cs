/**
 * File Name: ProceduralMesh.cs
 * Description: Script that handles the functionality of building / triangulating a procedural mesh
 * 
 * Authors: Will Lacey
 * Date Created: January 15, 2022
 * 
 * Additional Comments: 
 *      File Line Length: 120
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    /************************************************************/
    #region Class Enums

    private enum MeshShape { Procedural, Triangle, Quad, Cube, Tetrahedron, Sphere }

    #endregion
    /************************************************************/
    #region Fields

    [Header("Settings")]
    [SerializeField] MeshShape meshShape;

    #endregion
    /************************************************************/
    #region Properties

    public Mesh Mesh { get; set; }
    public MeshFilter MeshFilter => GetComponent<MeshFilter>();
    public MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
    public MeshCollider MeshCollider => GetComponent<MeshCollider>();
    
    public List<Vector3> Vertices { get; set; } = new List<Vector3>();
    public List<int> Triangles { get; set; } = new List<int>();
    public List<Vector3> UVs { get; set; } = new List<Vector3>();
    // public List<Vector3> Normals { get; set; } = new List<Vector3>();
    // public List<Vector3> Tangents { get; set; } = new List<Vector3>();

    #endregion
    /************************************************************/
    #region Functions

    #region Unity Functions

    private void Awake()
    {
        Mesh = new Mesh();
        MeshFilter.mesh = Mesh;
    }

    private void OnDestroy()
    {
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<MeshCollider>());
    }

    #endregion

    #region Mesh Functions

    public void Clear()
    {
        Mesh.Clear();
        Vertices.Clear();
        Triangles.Clear();

        UVs.Clear();
        // Normals.Clear();
        // Tangents.Clear();
    }

    public void Apply(bool hasMeshCollider = false)
    {
        Mesh.SetVertices(Vertices);
        Mesh.SetTriangles(Triangles, 0);

        if (UVs.Count != 0) Mesh.SetUVs(0, UVs);

        Mesh.RecalculateNormals();
        Mesh.RecalculateTangents();
        // if (Normals.Count != 0) Mesh.SetNormals(Normals);
        // if (Tangents.Count != 0) Mesh.SetTangents(Tangents);

        if (hasMeshCollider) MeshCollider.sharedMesh = Mesh;
    }

    public void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, int subdivisions = 0)
    {   
        // triangle layer count, vertex count, & triangle count (as floats to avoid casting)
        int lCount = (int) Mathf.Pow(2, subdivisions) + 1;
        int vCount = lCount * (lCount + 1) / 2;
        int tCount = (int) Mathf.Pow(4, subdivisions);

        Vertices.Add(v0);
        for (int l = 2; l <= lCount; l++)
        {
            Vector3 v01 = Vector3.Lerp(v0, v1, (l - 1f) / (lCount - 1f));
            Vector3 v02 = Vector3.Lerp(v0, v2, (l - 1f) / (lCount - 1f));

            int vIndex = Vertices.Count - 1;
            for (int v = 1; v <= l; v++) 
            {
                Vertices.Add(Vector3.Lerp(v01, v02, (v - 1f) / (l - 1f)));

                if (v == 1) continue;
                Triangles.Add(vIndex + v);          // current vertex                   
                Triangles.Add(vIndex + v - l);      // up right vertex neighbor
                Triangles.Add(vIndex + v - 1);      // right vertex neighbor
                if (v == l) continue;
                Triangles.Add(vIndex + v);          // current vertex
                Triangles.Add(vIndex + v - l + 1);  // up right vertex neighbor
                Triangles.Add(vIndex + v - l);      // up right vertex neighbor
            }
        }   
    }

    public void AddTriangleUV(Vector2 uv0, Vector2 uv1, Vector3 uv2, int subdivisions = 0)
    {
        // triangle layer count, vertex count, & triangle count (as floats to avoid casting)
        int lCount = (int) Mathf.Pow(2, subdivisions) + 1;
        int vCount = lCount * (lCount + 1) / 2;
        int tCount = (int) Mathf.Pow(4, subdivisions);

        UVs.Add(uv0);
        for (int l = 2; l <= lCount; l++)
        {
            Vector2 uv01 = Vector2.Lerp(uv0, uv1, (l - 1f) / (lCount - 1f));
            Vector2 uv02 = Vector2.Lerp(uv0, uv2, (l - 1f) / (lCount - 1f));
            
            for (int v = 1; v <= l; v++) UVs.Add(Vector2.Lerp(uv01, uv02, (v - 1f) / (l - 1f)));
        }   
    }

    public void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int subdivisions = 0)
    {
        // quad layer count, vertex count, & quad count (as floats to avoid casting)
        int lCount = (int) Mathf.Pow(2, subdivisions) + 1;
        int vCount = lCount * lCount;
        int sCount = (int) Mathf.Pow(4, subdivisions);

        for (int l = 1; l <= lCount; l++)
        {
            Vector3 v03 = Vector3.Lerp(v0, v3, (l - 1f) / (lCount - 1f));
            Vector3 v12 = Vector3.Lerp(v1, v2, (l - 1f) / (lCount - 1f));

            int vIndex = Vertices.Count - 1;
            for (int v = 1; v <= lCount; v++) 
            {
                Vertices.Add(Vector3.Lerp(v03, v12, (v - 1f) / (lCount - 1f)));

                if (l == 1 || v == 1) continue;
                Triangles.Add(vIndex + v);              // current vertex                   
                Triangles.Add(vIndex + v - 1);          // left vertex neighbor
                Triangles.Add(vIndex + v - lCount - 1); // up left vertex neighbor
                Triangles.Add(vIndex + v);              // current vertex                   
                Triangles.Add(vIndex + v - lCount - 1); // up left vertex neighbor
                Triangles.Add(vIndex + v - lCount);     // up vertex neighbor
            }
        }   
    }
    public void AddQuadUV(Vector2 uv0, Vector2 uv1, Vector3 uv2, Vector3 uv3, int subdivisions = 0) 
    {
        // quad layer count, vertex count, & quad count (as floats to avoid casting)
        int lCount = (int) Mathf.Pow(2, subdivisions) + 1;
        int vCount = lCount * lCount;
        int sCount = (int) Mathf.Pow(4, subdivisions);

        for (int l = 1; l <= lCount; l++)
        {
            Vector2 uv03 = Vector2.Lerp(uv0, uv3, (l - 1f) / (lCount - 1f));
            Vector2 uv12 = Vector2.Lerp(uv1, uv2, (l - 1f) / (lCount - 1f));

            for (int v = 1; v <= lCount; v++) UVs.Add(Vector3.Lerp(uv03, uv12, (v - 1f) / (lCount - 1f)));
        }   
	}

    #endregion

    #endregion
    /************************************************************/
    #region Debug
    #if DEBUG

    [Header("Debug Settings")]
    [Tooltip("size of shape")]
    [SerializeField, Range(0, 10)] int size = 0;
    [Tooltip("number of subdivisions per triangle for the shape")]
    [SerializeField, Range(0, 7)] int subdivisions = 0;
    [Tooltip("size of Gizmos spheres on shape's vertices")]
    [SerializeField, Min(0)] float gizmosVertexSize = 0.1f;

    private void OnValidate()
    {
        if (!Mesh) return;
        Debug.Log("Retriangulating");
        Start();
    }
    
    private void OnDrawGizmosSelected()
    {
        float shade = 0;
        for (int i = 0; i < Vertices.Count; i++) 
        {
            shade = (float) i / Vertices.Count;
            Gizmos.color = new Color(shade, shade, shade);
            Gizmos.DrawSphere(transform.TransformPoint(Vertices[i]), gizmosVertexSize);
            UnityEditor.Handles.Label(transform.TransformPoint(Vertices[i]), $"{i}");
        }
    }

    private void Start()
    {
        Clear();
        switch (meshShape)
        {
            case MeshShape.Triangle:
                this.TriangulateTriangle(size, subdivisions);
                break;
            case MeshShape.Quad:
                this.TriangulateQuad(size, subdivisions);
                break;
            case MeshShape.Cube:
                this.TriangulateCube(size, subdivisions);
                break;
            case MeshShape.Tetrahedron:
                this.TriangulateTetrahedron(size, subdivisions);
                break;
            case MeshShape.Sphere:
                this.TriangulateSphere(size, subdivisions);
                break;
        }
    }

    #endif
    #endregion
    /************************************************************/
}