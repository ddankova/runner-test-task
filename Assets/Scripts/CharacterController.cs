using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    public Transform rootPointsObj;

    GameObject boat = null;

    int nextPointIndex = 1; // change in char selector

    float speed = 10f;
    // coef that regulates the speed in different levels and surfaces
    float speedCoef = 1f;
    // the coef that regulates the speed of the character in slowdown moments
    float slowdownSpeedCoef = 0.6f;
    float slowdownDuration = 3f;
    float sideSpeed = 5f;
    float rotationSpeed = 5f; // Speed of rotation for smooth turning
    float rotationSpeedOnRiver = 1.5f; // Speed of rotation for smooth turning

    // the position that represents the character movement along the center of the root
    Vector3 charPosWithoutControlling;

    // accepted deviations from the center of the root
    float deviationOnRoad = 2.5f;
    float deviationOnRiverStart = 7f;
    float deviationOnRiverMedium = 5f;
    float deviationOnRiverSmallest = 4f;
    float currentDeviation = 2.5f;
    bool returningToCenter;

    float regularJumpHeight = 3f;
    float jumpHeightOnRiver = 6f;
    float regularJumpDuration = 0.8f;
    float jumpDurationOnRiver = 1.5f;

    bool onJump = false;
    bool jumpOnRiverEnabled = false;

    bool necessaryJump = false;
    // necessary jump missing is permited only once
    bool necessaryJumpMissed = false;
    float necessaryJumpCount = 0;

    string criticalPointDescription = null;

    // variables for under arch animation
    bool underArch = false;
    float underArchCameraAnimDuration = 4f;

    public bool gamePaused = false;
    bool gameEnded = false;

    CameraAnimationsController cameraAnimationsController = null;
    DialogManager dialogManager = null;
    ScoreManager scoreManager = null;
    BoatController boatController = null;
    ObjectsSpawner spawner = null;
    Animator animator = null;

    Collider charCollider = null;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != boat)
        {
            ProcessCollision(other);
        }
    }

    public void ProcessCollision(Collider other)
    {
        string tag = other.gameObject.tag;
        if (tag == "obstacle")
        {
            StartCoroutine(MistakeAnimation(0.5f, other.gameObject));

            if (scoreManager.lives == 0) EndGame();
        }
        else if (tag == "blueBonus") scoreManager.IncreaseScore(1);
        else if (tag == "greenBonus") scoreManager.IncreaseScore(2);
        else if (tag == "yellowBonus") scoreManager.IncreaseScore(3);
    }

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
        cameraAnimationsController = Camera.main.gameObject.GetComponent<CameraAnimationsController>();
        spawner = GameObject.FindWithTag("spawner").GetComponent<ObjectsSpawner>();

        GameObject dialogManagerObject = GameObject.FindWithTag("dialogManager");
        dialogManager = dialogManagerObject.GetComponent<DialogManager>();
        scoreManager = dialogManager.GetComponent<ScoreManager>();
        animator = GetComponent<Animator>();

        boat = GameObject.FindWithTag("boat");
        boatController = boat.GetComponent<BoatController>();

        charCollider = GetComponent<Collider>();

        currentDeviation = deviationOnRoad;
        boatController.character = gameObject;
        // initial set of obstacles and bonuses
        spawner.ResetSpawnObjects();
        charPosWithoutControlling = transform.position;

        scoreManager.ResetAll();
    }

    void Update()
    {
        if (!gamePaused)
        {
            float deltaTime = Time.deltaTime;
            Transform nextPointTransform = rootPointsObj.GetChild(nextPointIndex);

            // looking for critical points of the root
            List <string> dividedPointName = DivideString(nextPointTransform.name);
            if (dividedPointName != null)
            {
                string currentCriticalPointDescription = dividedPointName[1];

                if (currentCriticalPointDescription != criticalPointDescription)
                {
                    criticalPointDescription = currentCriticalPointDescription;

                    // set hints for jump on enter and exit river stage region

                    if (criticalPointDescription == "exitToRiver" || criticalPointDescription == "exitRiverPoint")
                    {
                        StartCoroutine(DecreaseSpeed(slowdownDuration * speedCoef));
                        StartCoroutine(dialogManager.ShowText("Jump!", slowdownDuration * speedCoef));

                        necessaryJump = true;
                    }
                    else if (criticalPointDescription == "jumpRiverPoint")
                    {
                        jumpOnRiverEnabled = true;
                    }
                    else if (criticalPointDescription == "endJumpRiverPoint")
                    {
                        jumpOnRiverEnabled = false;
                    }

                    // end region

                    // checking necessary jumps to and from river stage region

                    if (
                        (criticalPointDescription == "boatPoint" && necessaryJumpCount < 1) ||
                        (criticalPointDescription == "leaveBoatPoint" && necessaryJumpCount < 2)
                    )
                    {
                        if (!necessaryJumpMissed)
                        {
                            necessaryJumpMissed = true;

                            // on boat jump return to 61, from boat return to 93
                            float returnDuration = 2.5f;
                            StartCoroutine(ReturnToCheckpoint(nextPointIndex - 4, returnDuration));
                        }
                        else
                        {
                            EndGame();
                        }
                    }

                    // end region
                }
            }
            else if (criticalPointDescription != null) criticalPointDescription = null;

            Vector3 nextPointPosition = nextPointTransform.position;

            float frameSpeed = speed * speedCoef * deltaTime;

            // region with calculations of the natural character position without any player influence

            float distanceToNextPoint = (nextPointPosition - charPosWithoutControlling).magnitude;
            if (distanceToNextPoint < frameSpeed)
            {
                IncreaseNextPointIndex();
                nextPointPosition = rootPointsObj.GetChild(nextPointIndex).position;
            }

            Vector3 direction = nextPointPosition - charPosWithoutControlling;
            Vector3 posOffsetForward = direction.normalized * frameSpeed;

            charPosWithoutControlling += posOffsetForward;

            // Smoothly rotate toward the next point
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                (currentSurfaceType == SurfaceType.Road ? rotationSpeed : rotationSpeedOnRiver) * deltaTime
            );

            // end region

            // player controlling region

            // setting up deviation for different pieces of the root
            if (currentSurfaceType == SurfaceType.Road) currentDeviation = deviationOnRoad;
            else if (nextPointIndex >= 68 && nextPointIndex <= 75) currentDeviation = deviationOnRiverStart;
            else if (nextPointIndex >= 76 && nextPointIndex <= 81) currentDeviation = deviationOnRiverMedium;
            else if (nextPointIndex >= 82 && nextPointIndex <= 95) currentDeviation = deviationOnRiverSmallest;

            if (controllingDeviation.magnitude > currentDeviation && !returningToCenter) StartCoroutine(ReturnToCenter(0.5f));

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ControlCharacterSides(direction, -1, deltaTime);

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ControlCharacterSides(direction, 1, deltaTime);

            if (!onJump && Input.GetKey(KeyCode.Space))
            {
                if (necessaryJump)
                {
                    necessaryJumpCount++;

                    // checking if its the first necessary jump that is jump on boat
                    if (necessaryJumpCount == 1) StartCoroutine(JumpOnBoat(1.2f, jumpHeightOnRiver)); nextPointIndex = 68;
                    if (necessaryJumpCount == 2) StartCoroutine(JumpFromBoat(1.2f, jumpHeightOnRiver));
                }
                else if (currentSurfaceType == SurfaceType.Road || jumpOnRiverEnabled)
                {
                    float jumpHeight = currentSurfaceType == SurfaceType.Road ? regularJumpHeight : jumpHeightOnRiver;
                    float jumpDuration = currentSurfaceType == SurfaceType.Road ? regularJumpDuration : jumpDurationOnRiver;
                    StartCoroutine(Jump(jumpHeight, jumpDuration));
                }
            }

            // end region

            // set the new position of the character including controlling
            transform.position = charPosWithoutControlling + controllingDeviation + Vector3.up * jumpCoef;

            if (currentSurfaceType == SurfaceType.River)
            {
                boat.transform.position = transform.position - Vector3.up * jumpCoef;
                boat.transform.rotation = transform.rotation;
            }

            // start the under arch animation on critical points, arch index: 42-43, 54-55
            if ((nextPointIndex == 43 || nextPointIndex == 55) && !underArch)
            {
                StartCoroutine(cameraAnimationsController.MoveCameraUnderArch(underArchCameraAnimDuration));
                underArch = true;
            }
            if (nextPointIndex == 44 || nextPointIndex == 56) underArch = false;
        }
    }

    IEnumerator JumpFromBoat(float duration, float jumpHeight)
    {
        gamePaused = true;

        // first road point
        nextPointIndex = 96;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = rootPointsObj.GetChild(nextPointIndex).transform.position;
        
        yield return StartCoroutine(RotateCharacterOnRiver(targetPosition));

        // end region

        yield return StartCoroutine(NecessaryJumpAnimation(startPosition, targetPosition, duration, jumpHeight));

        animator.speed = 1; 
        gamePaused = false;
            
        necessaryJump = false;

        currentSurfaceType = SurfaceType.Road;
        boatController.ResetPosition();

        charCollider.enabled = true;
    }

    IEnumerator JumpOnBoat(float duration, float jumpHeight)
    {
        gamePaused = true;
        animator.speed = 0;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = boat.transform.position;

        yield return StartCoroutine(RotateCharacterOnRiver(targetPosition));

        // first river point
        nextPointIndex = 69;

        yield return StartCoroutine(NecessaryJumpAnimation(startPosition, targetPosition, duration, jumpHeight));

        gamePaused = false;
        necessaryJump = false;

        currentSurfaceType = SurfaceType.River;

        charCollider.enabled = false;
    }

    IEnumerator RotateCharacterOnRiver(Vector3 targetPosition)
    {
        float turnDuration = 0.2f;

        Quaternion initRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        // turn to the next point region
        float elapsedTime = 0f;

        while (elapsedTime < turnDuration)
        {
            elapsedTime += Time.deltaTime;

            // Interpolate rotation
            float t = elapsedTime / turnDuration; // Normalized time (0 to 1)
            Quaternion frameRotation = Quaternion.Slerp(initRotation, targetRotation, t);
            transform.rotation = frameRotation;
            if (currentSurfaceType == SurfaceType.River) boat.transform.rotation = frameRotation;

            yield return null; // Wait for the next frame
        }

        transform.rotation = targetRotation;
        if (currentSurfaceType == SurfaceType.River) boat.transform.rotation = targetRotation;
    }

    IEnumerator NecessaryJumpAnimation(Vector3 startPosition, Vector3 targetPosition, float duration, float jumpHeight)
    {
        float currentJumpHeight = 0;

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float jumpT = elapsedTime / (duration / 2);

            if (elapsedTime < duration / 2) currentJumpHeight = Mathf.Lerp(0, jumpHeight, jumpT);
            else currentJumpHeight = Mathf.Lerp(jumpHeight, 0, jumpT - 1);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t) + Vector3.up * currentJumpHeight;
            transform.LookAt(rootPointsObj.GetChild(nextPointIndex).position);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        charPosWithoutControlling = targetPosition;

        controllingDeviation = Vector3.zero;
    }

    IEnumerator Jump(float jumpHeight, float duration)
    {
        float elapsedTime = 0f;

        onJump = true;
        if (currentSurfaceType == SurfaceType.Road) animator.speed = 0;
        else { boatController.DecreaseColliderY(); };

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
        if (currentSurfaceType == SurfaceType.Road) animator.speed = 1;
        else { boatController.IncreaseColliderY(); };
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

        if (characterFutureDeviation.magnitude < currentDeviation)
        {
            controllingDeviation += rotatedVector.normalized * sideSpeed * deltaTime;
        }
    }

    Vector3 RotateVector(Vector3 vector, float angleInDegrees, Vector3 axis)
    {
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, axis); // Create rotation
        return rotation * vector; // Apply rotation to the vector
    }

    List<string> DivideString(string input)
    {
        if (string.IsNullOrEmpty(input) || !input.Contains("_"))
        {
            return null;
        }

        string[] parts = input.Split('_');
        return new List<string>(parts);
    }

    IEnumerator DecreaseSpeed(float duration)
    {
        float elapsedTime = 0f;
        float initSpeed = speed;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            speed = Mathf.Lerp(initSpeed, initSpeed * slowdownSpeedCoef, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            speed = Mathf.Lerp(initSpeed * slowdownSpeedCoef, initSpeed, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        speed = initSpeed;
    }

    IEnumerator ReturnToCheckpoint(int pointIndex, float duration)
    {
        gamePaused = true;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = rootPointsObj.GetChild(pointIndex).position;
        animator.speed = 0;

        float elapsedTime = 0f;

        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);

            Vector3 newPosition = Vector3.Lerp(currentPos, targetPos, t);

            transform.position = newPosition;
            if (currentSurfaceType == SurfaceType.River) boat.transform.position = newPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        nextPointIndex = pointIndex + 1;
        charPosWithoutControlling = targetPos;
        if (!gameEnded) { animator.speed = 1; gamePaused = false; }
    }

    IEnumerator ReturnToCenter(float duration)
    {
        returningToCenter = true;

        float elapsedTime = 0f;
        Vector3 initControllingDeviation = controllingDeviation;
        Vector3 targetControllingDeviation = Vector3.zero;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            controllingDeviation = Vector3.Lerp(initControllingDeviation, targetControllingDeviation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        controllingDeviation = targetControllingDeviation;

        returningToCenter = false;
    }

    void IncreaseNextPointIndex()
    {
        if (nextPointIndex == rootPointsObj.childCount - 1) { 
            nextPointIndex = 0;
            speedCoef += 0.1f;
            StartCoroutine(dialogManager.ShowText("Faster!", 1f));
            // resets obstacles and bonuses
            spawner.ResetSpawnObjects();
        }
        else nextPointIndex++;
    }

    public IEnumerator MistakeAnimation(float duration, GameObject destroyObstacle)
    {
        gamePaused = true;
        if (currentSurfaceType == SurfaceType.Road) animator.speed = 0;

        scoreManager.DecreaseLives();

        yield return StartCoroutine(dialogManager.ShowMistakeFrame(duration));
        Destroy(destroyObstacle);

        if (currentSurfaceType == SurfaceType.Road && !gameEnded) animator.speed = 1;
        if (!gameEnded) gamePaused = false;
    }

    public void ClearTheGame()
    {
        gamePaused = true;
        boatController.ResetPosition();
        spawner.DestroyAllSpawnedObjects();
        Destroy(GetComponent<CharacterController>());
        gameObject.SetActive(false);
    }

    void EndGame()
    {
        gameEnded = true;
        gamePaused = true;
        dialogManager.ShowEndScreen();
        
        charCollider.enabled = true;
    }
}
