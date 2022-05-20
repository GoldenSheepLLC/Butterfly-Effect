using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SphereJogger : MonoBehaviour
{
    /************************************************************/
    #region Fields

    [SerializeField] GameObject body = null;
    [SerializeField] GameObject nameTag = null;
    [SerializeField] GameObject deathParticleSystem = null;
    [SerializeField] Transform _orbitCameraAnchor = null;
    [SerializeField] RuntimeAnimatorController winAnimatorController = null;
    [SerializeField] RuntimeAnimatorController loseAnimatorController = null;

    #endregion
    /************************************************************/
    #region Properties

    public string Name
    {
        get => name;
        set => name = nameTag.GetComponentInChildren<TMP_Text>().text = value;
    }

    public Transform OrbitCameraAnchor => _orbitCameraAnchor;

    #endregion
    /************************************************************/
    #region Functions

    private void Update()
    {
        SetPosition();
        MovePosition();
    }

    private void SetPosition()
    {
        Vector3 origin = transform.position + 2 * transform.up;
        Vector3 direction = -transform.up; // transform.worldToLocalMatrix.MultiplyVector(transform.up);
        
        if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, SphereJoggerManager.Instance.SphereLayerMask))
        {
            Debug.DrawLine(origin, hit.point, Color.cyan);
            transform.position = hit.point + hit.normal * SphereJoggerManager.Instance.JoggerPositionBias;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else
        {
            Debug.DrawRay(origin, direction * 1000, Color.red);
            Debug.LogError("GetRaycastPoint() could not find point");
        }
    }

    private void MovePosition()
    {
        transform.position += transform.forward * SphereJoggerManager.Instance.JoggerSpeed * Time.deltaTime;
    }

    public void Death()
    {
        body.SetActive(false);
        nameTag.SetActive(false);
        deathParticleSystem.SetActive(true);
        GetComponent<CapsuleCollider>().enabled = false;
        Animator animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = null;
        enabled = false;

        string text = Name;
        FindObjectOfType<KillFeedDisplay>().Spawn(Name);
        StartCoroutine(CompleteDeath());
    }

    public void Win(int place)
    {
        deathParticleSystem.transform.parent = null;
        deathParticleSystem.SetActive(true);
        GetComponent<CapsuleCollider>().enabled = false;
        
        Animator animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = (place == 0) ? winAnimatorController : loseAnimatorController;
        enabled = false;

        string text = Name;
        FindObjectOfType<KillFeedDisplay>().Spawn(Name);

        transform.position = new Vector3(999, -0.1f * place, -place);
        transform.rotation = Quaternion.Euler(0, 90, 0);
    }

    private IEnumerator CompleteDeath()
    {
        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    #endregion
    /************************************************************/
}
