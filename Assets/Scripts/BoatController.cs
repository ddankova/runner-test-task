using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    public GameObject character;
    BoxCollider collider = null;

    Vector3 colliderSize = Vector3.zero;
    Vector3 colliderCenter = Vector3.zero;

    Vector3 initPosition = Vector3.zero;

    CharacterController characterController = null;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();
        colliderSize = collider.size;
        colliderCenter = collider.center;

        initPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (characterController == null) characterController = character.GetComponent<CharacterController>();
        GameObject colliderGameObject = other.gameObject;

        if (colliderGameObject != character)
        {
            StartCoroutine(characterController.MistakeAnimation(1f, other.gameObject));
        }
    }

    public void DecreaseColliderY()
    {
        collider.size = new Vector3(colliderSize.x, 0.8f, colliderSize.z);
        collider.center = new Vector3(colliderCenter.x, 0.4f, colliderCenter.z);
    }

    public void IncreaseColliderY()
    {
        collider.size = new Vector3(colliderSize.x, 2.7f, colliderSize.z);
        collider.center = new Vector3(colliderCenter.x, 1.45f, colliderCenter.z);
    }

    public void ResetPosition() { transform.position = initPosition; }
}
