/*
 * File Name: SphereManager.cs
 * Description: This script is for ...
 * 
 * Author(s): Kokowolo, Will Lacey
 * Date Created: April 28, 2022
 * 
 * Additional Comments:
 *		While this file has been updated to better fit this project, the original version can be found here:
 *		https://catlikecoding.com/unity/tutorials/hex-map/
 *
 *		File Line Length: 120
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Kokowolo.Utilities;
using Kokowolo.ProceduralMesh;

public class SphereManager : MonoBehaviour
{
    /************************************************************/
    #region Fields

    [Header("Manager Settings")]
    [SerializeField, Range(0, 10)] float bounds = 10;
    [SerializeField, Range(0, 10)] float offset = 0.5f;
    [SerializeField, Min(1)] int countX = 10;
    [SerializeField, Min(1)] int countY = 10;
    [SerializeField, Min(1)] int countZ = 10;

    [Header("Sphere Settings")]
    [SerializeField, Range(0, 2)] float sphereRadius = 0.5f;
    [SerializeField, Range(0, 6)] int sphereSubdivisions = 5;
    [SerializeField] Material _sphereMaterialDefault = null;
    [SerializeField] Material _sphereMaterialLasered = null;

    List<SphereMesh> spheres = new List<SphereMesh>();

    #endregion
    /************************************************************/
    #region Properties

    public static SphereManager Instance => Singleton<SphereManager>.Get();

    public Material SphereMaterialDefault => _sphereMaterialDefault;
    public Material SphereMaterialLasered => _sphereMaterialLasered;

    #endregion
    /************************************************************/
    #region Functions

    #region Unity Functions

    private void Awake()
    {
        Singleton<SphereManager>.Set(this, dontDestroyOnLoad: false);
        GenerateSpheres();
    }

    private void Start()
    {
        RegenerateSpheres();
    }

    #endregion

    #region Other Functions

    private void GenerateSpheres()
    {
        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    GenerateSphere(x, y, z);
                }
            }
        }
    }

    private void RegenerateSpheres()
    {
        int count = 0;
        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    RegenerateSphere(spheres[count], x, y, z);
                    count++;
                }
            }
        }
    }

    private void GenerateSphere(int x, int y, int z)
    {
        // create GameObject
        GameObject gameObject = new GameObject();
        gameObject.name = $"Sphere ({spheres.Count}) - ({x},{y},{z})";
        gameObject.transform.parent = transform;
        gameObject.layer = LayerMask.NameToLayer("Sphere");

        // create Sphere
        SphereMesh sphere = gameObject.AddComponent<SphereMesh>();
        gameObject.AddComponent<MeshCollider>();
        sphere.MeshRenderer.material = SphereMaterialDefault;
        spheres.Add(sphere);
    }

    private void RegenerateSphere(SphereMesh sphere, int x, int y, int z)
    {
        sphere.radius = sphereRadius;
        sphere.subdivisions = sphereSubdivisions;
        sphere.Refresh();

        Vector3 position = new Vector3(bounds, bounds, bounds) * -0.5f;
        
        position.x += Mathf.Lerp(0, bounds, (x + 1f) / (countX + 1f));
        position.y += Mathf.Lerp(0, bounds, (y + 1f) / (countY + 1f));
        position.z += Mathf.Lerp(0, bounds, (z + 1f) / (countZ + 1f));

        // position.z += (x % 2 == 0) ? 0.5f * offset : -0.5f * offset;
        // position.x += (y % 2 == 0) ? 0.5f * offset : -0.5f * offset;
        // position.y += (z % 2 == 0) ? 0.5f * offset : -0.5f * offset;
        if (x % 2 == 1) position.z += offset;
        if (y % 2 == 1) position.x += offset;
        if (z % 2 == 1) position.y += offset;

        sphere.transform.position = transform.position + position;
    }

    #endregion

    #endregion
    /************************************************************/
    #region Debug
    #if UNITY_EDITOR

    [MenuItem("CONTEXT/SphereManager/Reset Spheres")]
    public static void ResetSpheres(MenuCommand menuCommand)
    {
        if (!Application.isPlaying) return;

        foreach (ProceduralMesh sphere in Instance.spheres) 
        {
            Destroy(sphere.gameObject);
        }
        Instance.spheres.Clear();

        Instance.GenerateSpheres();
        Instance.RegenerateSpheres();
    }

    #endif
    #endregion
    /************************************************************/
}