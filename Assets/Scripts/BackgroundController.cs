using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public Camera mainCamera;
    public float distanceFromCamera = 11.0f;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;

            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0);
        }
    }
}
