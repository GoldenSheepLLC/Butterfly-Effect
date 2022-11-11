using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Kokowolo.Utilities;
using Kokowolo.ProceduralMesh;

public class SphereJoggerManager : MonoBehaviour
{
    /************************************************************/
    #region Fields

    [Header("Cached References")]
    [SerializeField] SphereMesh sphereMesh = null;
    [SerializeField] LineRenderer lineRenderer = null;
    [SerializeField] SphereJogger sphereJoggerPrefab = null;
    [SerializeField] WorldSpaceDisplay worldSpaceDisplay = null;
    [SerializeField] GameObject laserEnd = null;
    [SerializeField] TextAsset twitterFollowerDataTextAsset = null;
    [SerializeField] TextAsset instagramFollowerDataTextAsset = null;

    [Header("Settings")]
    [SerializeField] bool readTwitterData;
    [SerializeField, Range(0, 1)] float laserSize = 0.01f;
    [SerializeField, Range(0, 10)] float laserWaitTime = 0.2f;
    // [SerializeField, Range(0, 100)] float laserSpeed = 20f;
    [SerializeField, Range(0, 10)] float sphereSpeed = 0.1f;
    [SerializeField, Range(0, 1)] float sphereWaitTime = 0.01f;
    [SerializeField] LayerMask laserLayerMask;
    [SerializeField, Min(0)] int joggerMaxCount = 0;
    [SerializeField, Range(0, 10)] float _joggerPositionBias = 0.5f;
    [SerializeField, Range(0, 30)] float _joggerSpeed = 0.1f;
    [SerializeField] LayerMask _sphereLayerMask;

    List<string> followers = new List<string>();
    List<SphereJogger> sphereJoggers = new List<SphereJogger>();
    float laserCooldownTime = 2;
    float sphereCooldownTime = 2;
    bool orbitCameraToggle = true;
    // Vector3 currentLaserTravelDirection;

    #endregion
    /************************************************************/
    #region Properties

    public static SphereJoggerManager Instance => Singleton<SphereJoggerManager>.Get();

    public LayerMask SphereLayerMask => Instance._sphereLayerMask;
    public float JoggerPositionBias => Instance._joggerPositionBias;
    public float JoggerSpeed => Instance._joggerSpeed;

    #endregion
    /************************************************************/
    #region Functions

    #region Unity Functions

    private void Awake()
    {
        Singleton<SphereJoggerManager>.Set(this, dontDestroyOnLoad: false);

        lineRenderer.positionCount = 2;

        if (readTwitterData) ReadTwitterFollowerData();
        else ReadInstagramFollowerData();

        Vector3 position;
        Quaternion rotation;
        for (int i = 0; i < followers.Count && i < joggerMaxCount; i++)
        {
            position = new Vector3();
            rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            SphereJogger sphereJogger = Instantiate<SphereJogger>(sphereJoggerPrefab, position, rotation, transform);
            sphereJoggers.Add(sphereJogger);
            sphereJogger.gameObject.SetActive(true);
            sphereJogger.Name = $"{followers[i]}";
        }

        worldSpaceDisplay.SetText($"{sphereJoggers.Count}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            sphereSpeed += 0.001f;
            laserWaitTime -= 0.01f;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            sphereSpeed -= 0.001f;
            laserWaitTime += 0.01f;
        }
        if (Input.GetKeyDown(KeyCode.Space)) SwitchOrbitCameraTarget();

        if (Time.time > laserCooldownTime) UpdateLaser();
    }

    private void LateUpdate()
    {
        if (Time.time > sphereCooldownTime) UpdateSphereRadius();
    }

    #endregion

    #region Other Functions

    private void UpdateLaser()
    {
        laserCooldownTime = Time.time + laserWaitTime;
        
        Vector3 origin = transform.position;
        Vector3 direction;
        // if (Math.GetPercentRoll(0.01f))
        // {
            if (sphereJoggers.Count > 30) 
            {
                direction = sphereJoggers[Random.Range(0, sphereJoggers.Count)].transform.position;
                direction = (direction - origin).normalized;
            }
            else
            {
                direction = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)) * Vector3.up;
            }
            // currentLaserTravelDirection = direction;
            // currentLaserTravelDirection.z = 0;
            // currentLaserTravelDirection = currentLaserTravelDirection.normalized;
        // }
        // else
        // {
        //     direction = lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0);
        //     direction += currentLaserTravelDirection * laserSpeed * Time.deltaTime;
        // }
        
        Vector3 destination;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, laserLayerMask))
        {
            destination = hit.point;
            Debug.DrawLine(origin, destination, Color.yellow);

            if (!Physics.SphereCast(hit.point, 2 * laserSize, hit.normal, out hit, laserLayerMask)) return;

            SphereJogger sphereJogger = hit.transform.GetComponentInParent<SphereJogger>();
            if (sphereJogger) 
            {
                destination = sphereJogger.transform.position;
                HandleJoggerDeath(sphereJogger);
            }
        }
        else
        {
            destination = direction * 1000;
            Debug.DrawRay(origin, destination, Color.red);
            Debug.LogError("UpdateLaser() could not find point");
        }

        lineRenderer.startWidth = lineRenderer.endWidth = laserSize;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, destination);
        laserEnd.transform.position = destination;
    }

    private void UpdateSphereRadius()
    {
        sphereCooldownTime = Time.time + sphereWaitTime;

        if (sphereMesh.radius > -1) return;
        
        sphereMesh.transform.localScale *= sphereSpeed;// * Time.deltaTime;
        // sphereMesh.Refresh();
    }

    private void SwitchOrbitCameraTarget()
    {
        OrbitCamera orbitCamera = FindObjectOfType<OrbitCamera>();

        if (orbitCameraToggle)
        {
            orbitCamera.Target = sphereJoggers[Random.Range(0, sphereJoggers.Count)].OrbitCameraAnchor;
            orbitCamera.Distance = 3;
        }
        else
        {
            orbitCamera.Target = transform;
            orbitCamera.Distance = 35;
        }
        orbitCameraToggle = !orbitCameraToggle;
    }

    private void HandleJoggerDeath(SphereJogger jogger)
    {
        sphereJoggers.Remove(jogger);
        worldSpaceDisplay.SetText($"{sphereJoggers.Count}");

        if (sphereJoggers.Count < 10)
        {
            jogger.Win(sphereJoggers.Count);
            if (sphereJoggers.Count == 1)
            {
                enabled = false;
                OrbitCamera orbitCamera = FindObjectOfType<OrbitCamera>();

                jogger = sphereJoggers[Random.Range(0, sphereJoggers.Count)];
                jogger.Win(0);
                jogger.transform.position = new Vector3(1000, 0, 0);

                orbitCamera.Target = jogger.OrbitCameraAnchor;
                orbitCamera.Distance = 3;
            } 
        }
        else
        {
            jogger.Death();
        }
    }

    private void ReadTwitterFollowerData()
    {
        bool foundFollower = false;
        StreamReader reader = new StreamReader(new MemoryStream(twitterFollowerDataTextAsset.bytes)); 
        while(!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (line.Length != 0 && line[0] == '@') 
            {
                followers.Add(ReadWord(line, startIndex: 1));
                foundFollower = true;
            }
            else if (foundFollower)
            {
                if (!line.Contains("Follows you"))
                {
                    followers.RemoveAt(followers.Count - 1);
                }
                foundFollower = false;
            }
            else
            {
                foundFollower = false;
            }
            
        }
        reader.Close();
    }

    private void ReadInstagramFollowerData()
    {
        StreamReader reader = new StreamReader(new MemoryStream(instagramFollowerDataTextAsset.bytes)); 
        while(!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (line.Contains("username"))
            {
                int startIndex = line.IndexOf(":") + 3;
                followers.Add(ReadWord(line, startIndex));
            }
            
        }
        reader.Close();
    }

    private string ReadWord(string line, int startIndex)
    {
        string blacklist = " \"";
        string word = "";
        for (int i = startIndex; i < line.Length && !blacklist.Contains(line[i]); i++) word += line[i];
        return word;
    }

    #endregion

    #endregion
    /************************************************************/
    #region Debug
    #if UNITY_EDITOR

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(laserEnd.transform.position, 2 * laserSize);
    }

    #endif
    #endregion
    /************************************************************/
}
