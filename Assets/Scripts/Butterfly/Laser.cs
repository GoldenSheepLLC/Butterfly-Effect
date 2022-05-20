/*
 * File Name: Laser.cs
 * Description: This script is for ...
 * 
 * Author(s): Kokowolo, Will Lacey
 * Date Created: April 27, 2022
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

using Kokowolo.Utilities;
using Kokowolo.ProceduralMesh;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    /************************************************************/
    #region Fields

    [Header("Cached References")]
    [SerializeField] LineRenderer lineRenderer = null;
    [SerializeField] WorldSpaceDisplay laserAngleDisplay = null;
    [SerializeField] WorldSpaceDisplay bounceCountDisplay = null;
    [SerializeField] Transform laserBase = null;
    [SerializeField] Transform laserHead = null;
    [SerializeField] Transform laserEnd = null;
    [SerializeField] Material laserMaterial = null;

    [Header("Settings")]
    [SerializeField] LayerMask layerMask;
    [SerializeField, Range(0, 1)] float laserSize = 0.01f;
    [SerializeField, Range(0, 100)] int bounceMax = 10;
    [SerializeField, Range(0, 10)] float playerSpeed = 1;
    [SerializeField, Min(0)] float angleMin = 0.000000001f; // 11.4592982874 is ideal accuracy
    [SerializeField] bool stopAutoRotating = false;

    int bounceCount = 0;
    List<SphereMesh> spheres = new List<SphereMesh>(); 
    float angleAdjust;

    float laserBaseRotation = 0;
    float laserHeadRotation = 0;

    string text;

    #endregion
    /************************************************************/
    #region Properties

    public float Angle => transform.rotation.eulerAngles.x;
    public float BounceCount => bounceCount;

    #endregion
    /************************************************************/
    #region Functions

    #region Unity Functions

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        ManualRotate();

        if (Input.GetKeyDown(KeyCode.T)) stopAutoRotating = !stopAutoRotating;
        if (stopAutoRotating) return;
        Rotate();
        
    }

    private void LateUpdate()
    {
        text = $"{laserBase.localEulerAngles.z.ToString("F5")}º\n{laserHead.localEulerAngles.x.ToString("F5")}º";
        laserAngleDisplay.SetText(text);
        text = (BounceCount > 9) ? $"Bounces: {BounceCount}" : $"Bounces: 0{BounceCount}";
        bounceCountDisplay.SetText(text);
        Refresh();
    }

    #endregion

    #region Other Functions

    private void ManualRotate()
    {
        if (lineRenderer.positionCount > 21 && !Input.GetKey(KeyCode.LeftShift)) return;

        if (Input.GetKey(KeyCode.Alpha1)) laserBaseRotation = Mathf.Lerp(laserBaseRotation, -1, Time.deltaTime);
        else if (Input.GetKey(KeyCode.Alpha2)) laserBaseRotation = Mathf.Lerp(laserBaseRotation, 1, Time.deltaTime);
        else laserBaseRotation = 0;

        if (Input.GetKey(KeyCode.Alpha3)) laserHeadRotation = Mathf.Lerp(laserHeadRotation, -1, Time.deltaTime);
        else if (Input.GetKey(KeyCode.Alpha4)) laserHeadRotation = Mathf.Lerp(laserHeadRotation, 1, Time.deltaTime);
        else laserHeadRotation = 0;

        laserBase.Rotate(new Vector3(0, 0, laserBaseRotation) * playerSpeed);
        laserHead.Rotate(new Vector3(laserHeadRotation, 0, 0) * playerSpeed);
    }

    public void Rotate()
    {
        angleAdjust = angleMin * Time.deltaTime;
        
        laserBase.Rotate(new Vector3(0, 0, angleAdjust));
        laserHead.Rotate(new Vector3(angleAdjust, 0, 0));

        // transform.Rotate(new Vector3(0, angleAdjust, 0));

        // Debug.Log(transform.rotation.eulerAngles.ToString("F16"));
        //Debug.Log(transform.rotation.eulerAngles.x);
    }

    public void Refresh()
    {
        lineRenderer.startWidth = lineRenderer.endWidth = laserSize;
        lineRenderer.positionCount = 0;
        bounceCount = 0;

        Vector3 origin = transform.position;
        // Vector3 direction = transform.parent.InverseTransformDirection(transform.forward);
        // Vector3 direction = transform.TransformDirection(transform.forward);
        Vector3 direction = transform.forward;//transform.TransformDirection(transform.forward);
        AddLaserPoint(origin);
        ClearSpheres();
        DoLaserBounce(origin, direction);
    }

    private void DoLaserBounce(Vector3 origin, Vector3 directionIn)
    {
        if (Physics.Raycast(origin, directionIn, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Vector3 directionOut = GetBounceOutVector(directionIn, hit.normal);
            //Debug.DrawRay(transform.position, direction * 10f, Color.green);
            //Debug.DrawRay(hit.point, hit.normal * 10f, Color.yellow);
            //Debug.DrawRay(hit.point, directionOut * 10f, Color.red);
            if (hit.transform.tag == "Bounds")
            {
                AddLaserPoint(hit.point);
                laserEnd.transform.position = hit.point;
            }
            else
            {
                AddLaserPoint(hit.point);
                TryAddSphere(hit.transform);
                if (bounceCount++ < bounceMax) DoLaserBounce(hit.point, directionOut);
            }
        }
        else
        {
            AddLaserPoint(origin + directionIn * 150f);
            laserEnd.transform.position = origin + directionIn * 150f;
            //Debug.DrawRay(origin, directionIn * 1000, Color.white);
        }
    }

    private Vector3 GetBounceOutVector(Vector3 v, Vector3 n)
    {
        // thanks to Gareth Rees's answer https://stackoverflow.com/questions/573084/how-to-calculate-bounce-angle
        Vector3 u = Vector3.Dot(v, n) * n;
        Vector3 w = v - u;
        return w - u;
    }

    private void AddLaserPoint(Vector3 point)
    {
        int index = lineRenderer.positionCount;
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(index, point);

        // float t = Mathf.Clamp01(lineRenderer.positionCount / 30f);
        if (lineRenderer.positionCount > 21)
        {
            Color color = Kokowolo.Utilities.Math.GetRandomColor();
            laserMaterial.SetColor("_Color", color * 24);
        }
        
        // lineRenderer.startColor = Color.red;
        // lineRenderer.endColor = Color.Lerp(Color.red, Color.cyan, t);
    }

    private void TryAddSphere(Transform transform)
    {
        if (!transform.TryGetComponent<SphereMesh>(out SphereMesh sphere)) return;
        if (spheres.Contains(sphere)) return;
        
        sphere.MeshRenderer.material = SphereManager.Instance.SphereMaterialLasered;
        spheres.Add(sphere);
    }

    private void ClearSpheres()
    {
        foreach (SphereMesh sphere in spheres)
        {
            sphere.MeshRenderer.material = SphereManager.Instance.SphereMaterialDefault;
        }
        spheres.Clear();
    }

    #endregion

    #endregion
    /************************************************************/
    #region Debug
    #if UNITY_EDITOR

    [Header("Debug")]
    [SerializeField, Range(0, 2)] float gizmosSphereRadius = 0.1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Gizmos.DrawSphere(lineRenderer.GetPosition(i), gizmosSphereRadius * 0.5f);
        }
    }

    #endif
    #endregion
    /************************************************************/
}