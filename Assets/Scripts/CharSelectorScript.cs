using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharSelectorScript : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] float distanceFromCamera = 900;
    [SerializeField] GameObject animalsParent;
    [SerializeField] Transform rootPoints;

    [SerializeField] GameObject chooseCharScreen;
    [SerializeField] GameObject gameScreen;
    [SerializeField] Button confirmBtn;

    [SerializeField] Vector3 startCameraRot = new Vector3(-48, 13, 0);

    List<GameObject> chars = new List<GameObject>();
    bool charChosen = false;
    GameObject confirmedChar = null; // character to play with
    GameObject chosenChar = null; // chosen character, needs to be confirmed

    float chosenCharScale = 7.2f;
    float notChosenCharsScale = 4.8f;
    float charNormalScale = 6f;

    Vector3 charStartRotation = new Vector3(0, -90, 0);
    Vector3 charGameScale = new Vector3(0.045f, 0.045f, 0.045f);

    Vector3 animalParentChoicePosition = Vector3.zero;

    List<Vector3> charSelectionPositions = new List<Vector3>()
    {
        new Vector3 (-600, -100, 0),
        new Vector3 (-200, -100, 0),
        new Vector3 (200, -100, 0),
        new Vector3 (600, -100, 0)
    };

    Vector3 charSelectionRotation = new Vector3 (20, 0, 0);

    bool selection = false;

    void Start()
    {
        animalsParent.GetComponent<AudioSource>().Play();
    }

    public void StartSelection()
    {
        chosenChar = null;
        selection = true;
        
        // setting the characters parent obj before the camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        mainCamera.gameObject.transform.eulerAngles = startCameraRot;

        Transform animalsParentTransform = animalsParent.transform;

        animalParentChoicePosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;

        animalsParentTransform.position = animalParentChoicePosition;

        animalsParentTransform.LookAt(mainCamera.transform);

        chars = GetAllChildObjects(animalsParent);

        chars.ForEach(character => {
            character.SetActive(true);
            Transform charTransform = character.transform;
            charTransform.localScale = Vector3.one * charNormalScale;
            charTransform.localPosition = charSelectionPositions[chars.IndexOf(character)];
            charTransform.localRotation = Quaternion.Euler(charSelectionRotation);
        });

        chooseCharScreen.SetActive(true);
        gameScreen.SetActive(false);

        StartCoroutine(RotateCharacters());
    }

    void Update()
    {
        // Check if the left mouse button is pressed
        if (Input.GetMouseButtonDown(0) && selection)
        {
            Vector3 mousePosition = Input.mousePosition;

            // Call the DetectTap method with the mouse position
            DetectClick(mousePosition);
        }
    }

    void DetectClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject tappedObject = hit.collider.gameObject;

            OnCharacterTapped(tappedObject);
        }
    }

    void OnCharacterTapped(GameObject tappedChar)
    {
        if (tappedChar == chosenChar) ConfirmCharacterAndSetItToStart();
        else
        {
            if (!confirmBtn.gameObject.activeSelf)
            {
                confirmBtn.gameObject.SetActive(true);

                confirmBtn.onClick.AddListener(() =>
                {
                    ConfirmCharacterAndSetItToStart();
                });
            }

            chosenChar = tappedChar;
            StartCoroutine(ScaleObject(chosenChar, chosenCharScale, 0.5f));

            foreach (GameObject character in chars)
            {
                if (character != chosenChar) StartCoroutine(ScaleObject(character, notChosenCharsScale, 0.5f));
            }
        };
    }

    IEnumerator ScaleObject(GameObject obj, float endScale, float duration)
    {
        Vector3 initialScale = obj.transform.localScale;

        Vector3 targetScale = new Vector3(endScale, endScale, endScale);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor
            float t = elapsedTime / duration;

            // Interpolate between the start and end scales
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the object is exactly at the target scale at the end
        obj.transform.localScale = targetScale;
    }

    // ConfirmCharacter method confirms the character choice, exits the screen snd starts the game
    void ConfirmCharacterAndSetItToStart()
    {
        confirmedChar = chosenChar;

        confirmBtn.onClick.RemoveAllListeners();
        confirmBtn.gameObject.SetActive(false);
        chooseCharScreen.SetActive(false);
        gameScreen.SetActive(true);
        StopAllCoroutines();

        foreach (GameObject character in chars)
        {
            if (character != confirmedChar) character.gameObject.SetActive(false);
        }

        CharacterController charController = confirmedChar.AddComponent<CharacterController>();
        charController.rootPointsObj = rootPoints;

        animalsParent.transform.position = Vector3.zero;
        animalsParent.transform.rotation = Quaternion.identity;

        confirmedChar.transform.position = rootPoints.GetChild(0).position;
        confirmedChar.transform.eulerAngles = charStartRotation;
        confirmedChar.transform.localScale = charGameScale;

        mainCamera.gameObject.GetComponent<CameraAnimationsController>().StartTheGameAnimation(confirmedChar);

        selection = false;
    }

    public void SetTheSameCharacterToGame()
    {
        confirmedChar.SetActive(true);
        CharacterController charController = confirmedChar.AddComponent<CharacterController>();
        charController.rootPointsObj = rootPoints;

        animalsParent.transform.position = Vector3.zero;
        animalsParent.transform.rotation = Quaternion.identity;

        confirmedChar.transform.position = rootPoints.GetChild(0).position;
        confirmedChar.transform.eulerAngles = charStartRotation;
        confirmedChar.transform.localScale = charGameScale;

        mainCamera.gameObject.GetComponent<CameraAnimationsController>().StartTheGameAnimation(confirmedChar);
    }

    IEnumerator RotateCharacters()
    {
        float rotationSpeed = 0.08f;

        while (!charChosen)
        {
            foreach (GameObject character in chars)
            {
                character.transform.Rotate(Vector3.up, rotationSpeed);
            }

            yield return null;
        }
    }

    List<GameObject> GetAllChildObjects(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
        }

        return children;
    }
}
