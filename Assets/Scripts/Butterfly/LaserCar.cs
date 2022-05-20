// /*
//  * File Name: LaserCar.cs
//  * Description: This script is for ...
//  * 
//  * Author(s): Kokowolo, Will Lacey
//  * Date Created: May 4, 2022
//  * 
//  * Additional Comments:
//  *		File Line Length: 120
//  */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class LaserCar : MonoBehaviour
// {
//     /************************************************************/
//     #region Fields

//     [Header("Cached References")]
//     [SerializeField] Transform gunBase;
//     [SerializeField] Transform gunHead;

//     Vector2 playerInput;

//     #endregion
//     /************************************************************/
//     #region Functions

//     #region Unity Functions

//     private void Awake()
//     {
        
//     }

//     private void Start()
//     {

//     }

//     private void Update()
//     {
//         playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

//         transform.position 

//         Debug.Log(playerInput);
//     }

//     private void LateUpdate()
//     {
        
//     }

//     #endregion

//     #endregion
//     /************************************************************/
// }

/**
 * File Name: MovingSphere.cs
 * Description: This class is an example for templated movement on an actor in the scene
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: March 9, 2021
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/movement/ within Catlike Coding's tutorial series:
 *      Movement; this file has been updated it to better fit this project
 **/

/// <summary>
/// an example for templated movement on an actor in the scene
/// </summary>
public class LaserCar : MonoBehaviour
{
	/************************************************************/
	#region Variables

	[Header("Controls")]
	[Tooltip("the player's movement input relative to some transform's point of view")]
	[SerializeField] Transform playerInputSpace = default;

	/* class params */
	[Header("Movement")]
	[SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)] float rotationSpeed = 10f;

	/* movement variables */
	Vector3 velocity;
	Vector3 targetVelocity;

	/* moving body variables */
	Vector3 connectionVelocity;
	Vector3 connectionWorldPosition;
	Vector3 connectionLocalPosition;

	/* gravity variables */
	Vector3 upAxis;
	Vector3 rightAxis;
	Vector3 forwardAxis;

	/* class 'controls' variables */
	Vector2 playerInput;
    bool cameraToggle = false;
    [SerializeField] Camera orbitCamera = null;
    [SerializeField] Camera carCamera = null;

	#endregion
    /************************************************************/
	#region Properties

    Camera Camera { get; set; }

    #endregion
	/************************************************************/
	#region Class Functions

	#region Unity Functions

    private void Awake() 
    {
        Camera = orbitCamera;
        playerInputSpace = Camera.transform;
    }

	private void Update()
	{
		playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Camera.gameObject.SetActive(false);
            Camera = (cameraToggle) ?  orbitCamera : carCamera;
            Camera.gameObject.SetActive(true);
            cameraToggle = !cameraToggle;
            playerInputSpace = Camera.transform;
        }

		if (playerInputSpace)
		{
			rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
		}
		else
		{
			rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
		}

		targetVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
	}

    private void FixedUpdate()
    {
		AdjustVelocity();

		// instantaneous change is generally bad, but here we're adding an accelertion to the
		//	velocity so it's fine
		transform.position += velocity;

        if (velocity.magnitude > 0.05f)
        {
            Vector3 relativePos = (transform.position + velocity) - transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);
            Quaternion current = transform.localRotation;
            transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime * rotationSpeed);
        }

        // transform.LookAt(velocity, Vector3.up);
	}
    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, transform.position + velocity * 10);
    }

	#endregion
	
	#region Movement Functions

	private void AdjustVelocity()
	{
		Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, Vector3.up);
		Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, Vector3.up);

		Vector3 relativeVelocity = velocity - connectionVelocity;
		float currentX = Vector3.Dot(relativeVelocity, xAxis);
		float currentZ = Vector3.Dot(relativeVelocity, zAxis);

		float acceleration = maxAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX = Mathf.MoveTowards(currentX, targetVelocity.x, maxSpeedChange);
		float newZ = Mathf.MoveTowards(currentZ, targetVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}

	#endregion

	#region Utility Functions

	/// <summary>
	/// https://catlikecoding.com/unity/tutorials/movement/physics/slopes/projecting-vector.png
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="normal"></param>
	/// <returns></returns>
	private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
	{
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
	}

	#endregion

	#endregion
}