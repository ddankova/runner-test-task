using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraAnimationsController : MonoBehaviour
{
    [SerializeField] GameObject background;

    GameObject character;
    CharacterController charController;

    float gameplayDistanceToChar = 7;

    bool startAnimationsEnded = false;

    // camera to be a little upper than the character
    float correctiveCameraCoef = 8f;
    // camera to be down under the arches
    float archCorrectiveCameraCoef = 3f;

    // camera to look a little upper than the character point
    Vector3 correctingCameraLookAtVector = Vector3.up * 3;

    Vector3 initCameraPosition = Vector3.zero;

    private void Start()
    {
        initCameraPosition = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        if (startAnimationsEnded) SetCameraBehindChar();
    }

    public void ResetCameraPosition()
    {
        transform.position = initCameraPosition;
        startAnimationsEnded = false;
        background.SetActive(true);
    }

    void SetCameraBehindChar()
    {
        Vector3 charPos = character.transform.position;
        Vector3 targetPosition = charPos - character.transform.forward * gameplayDistanceToChar + (Vector3.up * correctiveCameraCoef);

        transform.position = targetPosition;
        transform.LookAt(charPos + correctingCameraLookAtVector);
    }

    /// <summary>
    /// The first launching of the game
    /// </summary>
    /// <param name="character"></param> confirmed character to play
    public void StartTheGameAnimation(GameObject character)
    {
        this.character = character;
        charController = character.GetComponent<CharacterController>();

        StartCoroutine(StartCameraAnimation());
    }

    IEnumerator StartCameraAnimation()
    {
        // initial camera rotation
        yield return StartCoroutine(RotateCameraToLookAtCharacter(1f));

        StartCoroutine(StartCameraMovement());

        yield return new WaitForSeconds(3);

        background.SetActive(false);

        startAnimationsEnded = true;
    }

    IEnumerator StartCameraMovement()
    {
        float rotationDuration = 1.5f;
        float zoomInDuration = 1.5f;

        yield return StartCoroutine(RotateAroundCharacter(rotationDuration));
        yield return StartCoroutine(ZoomInCharacter(zoomInDuration));
        
    }

    /// <summary>
    /// Smoothly rotates the camera around the character while zooming in.
    /// </summary>
    /// <param name="duration"></param> Duration of the animation
    /// <returns></returns>
    IEnumerator RotateAroundCharacter(float duration)
    {
        float elapsedTime = 0f;

        // Initial values
        Vector3 startPosition = transform.position;

        while (elapsedTime < duration)
        {
            Vector3 charPos = character.transform.position;
            float startDistance = (startPosition - charPos).magnitude;

            // Target position (behind the character, at the desired distance)
            Vector3 targetPosition = charPos - character.transform.forward * gameplayDistanceToChar + Vector3.up * (startPosition.y - charPos.y);

            // Initial and final angles for rotation
            float startAngle = Mathf.Atan2(startPosition.z - charPos.z, startPosition.x - charPos.x) * Mathf.Rad2Deg;
            float endAngle = Mathf.Atan2(targetPosition.z - charPos.z, targetPosition.x - charPos.x) * Mathf.Rad2Deg;

            float t = elapsedTime / duration;

            // Interpolate the angle smoothly
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

            // Calculate the new camera position along the circular arc
            float radianAngle = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radianAngle) * startDistance, 0, Mathf.Sin(radianAngle) * startDistance);

            // Update the camera position
            transform.position = charPos + offset + Vector3.up * (startPosition.y - charPos.y);

            // Make the camera look at the character
            transform.LookAt(charPos + correctingCameraLookAtVector);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ZoomInCharacter(float duration)
    {
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            Vector3 charPos = character.transform.position;
            Vector3 targetPosition = charPos - character.transform.forward * gameplayDistanceToChar + Vector3.up * correctiveCameraCoef;

            // Smooth interpolation (use Mathf.SmoothStep or an easing function for better effect)
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);

            transform.position = newPosition;

            transform.LookAt(charPos + correctingCameraLookAtVector);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetCameraBehindChar();
    }

    /// <summary>
    /// Smoothly rotates the camera to look at the character.
    /// </summary>
    /// <param name="duration"></param> Duration of the animation
    /// <returns></returns>
    IEnumerator RotateCameraToLookAtCharacter(float duration)
    {
        Quaternion startRotation = transform.rotation; // Initial rotation of the camera
        Quaternion targetRotation = Quaternion.LookRotation(character.transform.position - transform.position); // Rotation to look at the character

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Smoothly interpolate the rotation using Quaternion.Slerp
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure the camera directly looks at the character at the end
        transform.rotation = targetRotation;
    }

    public IEnumerator MoveCameraUnderArch(float duration)
    {
        float elapsedTime = 0f;
        float startCorrectiveCameraCoef = correctiveCameraCoef;
        charController.underArch = true;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            correctiveCameraCoef = Mathf.Lerp(startCorrectiveCameraCoef, archCorrectiveCameraCoef, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            correctiveCameraCoef = Mathf.Lerp(archCorrectiveCameraCoef, startCorrectiveCameraCoef, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        charController.underArch = false;
    }
}
