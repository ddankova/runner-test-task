using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionController : MonoBehaviour
{
    [SerializeField] Transform highlightsParent;
    [SerializeField] GameObject page0;
    [SerializeField] GameObject page1;
    [SerializeField] GameObject dialogWingow;

    int currentPageIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeInAndOutHighlights(3f, 0.7f));
    }

    public void OpenNextPage()
    {
        if (currentPageIndex == 0)
        {
            StopAllCoroutines();
            page0.SetActive(false);
            page1.SetActive(true);

            currentPageIndex++;
        }
        else if (currentPageIndex == 1)
        {
            page1.SetActive(false);
            dialogWingow.SetActive(true);

            currentPageIndex++;
        }
    }

    IEnumerator FadeInAndOutHighlights (float duration, float maxAlpha)
    {
        float halfDuration = duration / 2f;
        Color originalColor = highlightsParent.GetChild(0).gameObject.GetComponent<Image>().color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        for (int i = 0; i < highlightsParent.childCount; i++)
        {
            GameObject childObj = highlightsParent.GetChild(i).gameObject;
            Image childImage = childObj.GetComponent<Image>();

            childImage.color = transparentColor;
            childObj.gameObject.SetActive(true);
        }
        
        while(true)
        {
            // Fade In
            float elapsedTime = 0f;
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / halfDuration * maxAlpha);

                for (int i = 0; i < highlightsParent.childCount; i++)
                {
                    Image childImage = highlightsParent.GetChild(i).gameObject.GetComponent<Image>();

                    childImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                yield return null;
            }

            // Hold at full opacity for a moment if needed
            yield return new WaitForSeconds(0.1f);

            // Fade Out
            elapsedTime = 0f;
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsedTime / halfDuration * maxAlpha));

                for (int i = 0; i < highlightsParent.childCount; i++)
                {
                    Image childImage = highlightsParent.GetChild(i).gameObject.GetComponent<Image>();

                    childImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                yield return null;
            }

            yield return null;
        }
    }
}
