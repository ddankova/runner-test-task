using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterController : MonoBehaviour
{
    public Transform rootPointsObj;
    public bool charMoving;

    GameObject boat = null;

    int nextPointIndex = 1;

    float speed = 10f;
    float sideSpeed = 5f;
    float rotationSpeed = 5f; // Speed of rotation for smooth turning

    // arch index: 52-53 42-43

    // the position that represents the character movement along the center of the root
    Vector3 charPosWithoutControlling;

    // accepted deviations from the center of the root
    float deviationOnRoad = 2.2f;
    float deviationOnRiver = 1.5f;
    float deviationOnRiverStart = 10f;

    float regularJumpHeight = 3f;
    float jumpDuration = 0.8f;
    bool onJump = false;

    public bool underArch = false;
    float underArchCameraAnimDuration = 4f;

    CameraAnimationsController cameraAnimationsController = null;
    Animator animator = null;

    // the vector that represents the movement to right and left
    Vector3 controllingDeviation = Vector3.zero;
    // the float that represents current jump height
    float jumpCoef = 0f;

    enum SurfaceType
    {
        River,
        Road
    }

    SurfaceType currentSurfaceType = SurfaceType.Road;

    private void OnEnable()
    {
        GetComponent<Animator>().enabled = true;
    }

    private void OnDisable()
    {
        GetComponent<Animator>().enabled = false;
    }

    private void Start()
    {
        charPosWithoutControlling = transform.position;
        cameraAnimationsController = Camera.main.gameObject.GetComponent<CameraAnimationsController>();
        animator = GetComponent<Animator>();

        boat = GameObject.FindGameObjectWithTag("boat");
    }

    void Update()
    {
        if (charMoving)
        {
            float deltaTime = Time.deltaTime;
            Vector3 nextPointPosition = rootPointsObj.GetChild(nextPointIndex).position;

            float frameSpeed = speed * deltaTime;

            float distanceToNextPoint = (nextPointPosition - charPosWithoutControlling).magnitude;
            if (distanceToNextPoint < frameSpeed)
            {
                nextPointIndex++;
                nextPointPosition = rootPointsObj.GetChild(nextPointIndex).position;
            }

            if (nextPointIndex == rootPointsObj.childCount - 1) nextPointIndex = 0;

            Vector3 direction = nextPointPosition - charPosWithoutControlling;
            Vector3 posOffsetForward = direction.normalized * frameSpeed;

            // Smoothly rotate toward the next point
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow)) ControlCharacterSides(direction, -1, deltaTime);

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow)) ControlCharacterSides(direction, 1, deltaTime);

            if (!onJump && Input.GetKey(KeyCode.Space)) StartCoroutine(Jump(regularJumpHeight, jumpDuration));

            charPosWithoutControlling += posOffsetForward;
            transform.position = charPosWithoutControlling + controllingDeviation + Vector3.up * jumpCoef;

            // start the under arch animation on critical points
            if ((nextPointIndex == 43 || nextPointIndex == 55) && !underArch) StartCoroutine(cameraAnimationsController.MoveCameraUnderArch(underArchCameraAnimDuration));
        }
    }

    IEnumerator Jump(float jumpHeight, float duration)
    {
        float elapsedTime = 0f;

        onJump = true;
        animator.speed = 0;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            jumpCoef = Mathf.Lerp(0, jumpHeight, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            jumpCoef = Mathf.Lerp(jumpHeight, 0, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        onJump = false;
        animator.speed = 1;
    }

    /// <summary>
    /// Controls the movement of character from side to side
    /// </summary>
    /// <param name="coef"></param> coef to choose the left or right side
    void ControlCharacterSides(Vector3 direction, int coef, float deltaTime)
    {
        // vector that represents the direction of the character movement
        Vector3 rotatedVector = RotateVector(direction, 90f * coef, Vector3.up);
        // character deviation from the center of the root, if the movement will be accepted
        Vector3 characterFutureDeviation = controllingDeviation + rotatedVector.normalized * sideSpeed * deltaTime;

        if (characterFutureDeviation.magnitude < deviationOnRoad)
        {
            controllingDeviation += rotatedVector.normalized * sideSpeed * deltaTime;
        }
    }

    Vector3 RotateVector(Vector3 vector, float angleInDegrees, Vector3 axis)
    {
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, axis); // Create rotation
        return rotation * vector; // Apply rotation to the vector
    }
}
