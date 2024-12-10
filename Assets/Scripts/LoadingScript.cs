using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScript : MonoBehaviour
{
    [SerializeField] GameObject LoadingScreen;
    [SerializeField] int sceneId;
    [SerializeField] GameObject horsePrefab;
    [SerializeField] GameObject loadingHorsesParent;

    List<Dictionary<string, Vector2>> loadingHorseAnchors = new List<Dictionary<string, Vector2>>()
    {
        new Dictionary<string, Vector2>() {
            { "min", new Vector2(0.08f, 0.12f) }, { "max", new Vector2(0.29f, 0.88f) }
        },
        new Dictionary<string, Vector2>() {
            { "min", new Vector2(0.39f, 0.12f) }, { "max", new Vector2(0.6f, 0.88f) }
        },
        new Dictionary<string, Vector2>() {
            { "min", new Vector2(0.71f, 0.12f) }, { "max", new Vector2(0.92f, 0.88f) }
        }
    };

    List<GameObject> initiatedHorses = new List<GameObject> ();


    private void Awake()
    {
        LoadScene(sceneId);
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        LoadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            int newHorseIndex = initiatedHorses.Count;
            if (newHorseIndex >= 3)
            {
                initiatedHorses.ForEach(horse => { Destroy(horse); });
                initiatedHorses.Clear();
                newHorseIndex = 0;
            }

            GameObject horse = Instantiate(horsePrefab, Vector3.zero, Quaternion.identity, loadingHorsesParent.transform);
            initiatedHorses.Add(horse);

            RectTransform rectTransform = horse.GetComponent<RectTransform>();
            rectTransform.anchorMin = loadingHorseAnchors[newHorseIndex]["min"];
            rectTransform.anchorMax = loadingHorseAnchors[newHorseIndex]["max"];
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            yield return new WaitForSeconds(0.3f);
        }
    }
}
