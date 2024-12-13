using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogText;
    [SerializeField] GameObject endScreen;
    [SerializeField] Text scoreText;
    [SerializeField] Text bestScoreText;
    [SerializeField] Image mistakeImage;

    public IEnumerator ShowText(string text, float duration)
    {
        // Ensure jumpText is active
        dialogText.SetActive(true);

        RectTransform rectTransform = dialogText.GetComponent<RectTransform>();
        dialogText.GetComponent<Text>().text = text;
        if (rectTransform == null)
        {
            Debug.LogError("jumpText does not have a RectTransform component!");
            yield break;
        }

        // Initial setup
        Vector3 offScreenLeft = new Vector3(-Screen.width, rectTransform.anchoredPosition.y, 0);
        Vector3 centerScreen = new Vector3(0, rectTransform.anchoredPosition.y, 0);
        Vector3 offScreenRight = new Vector3(Screen.width, rectTransform.anchoredPosition.y, 0);

        rectTransform.anchoredPosition = offScreenLeft;
        rectTransform.localScale = Vector3.one;

        float halfDuration = duration / 2f;
        float scaleUpTime = halfDuration / 2f;

        // Move from left to center
        yield return StartCoroutine(MoveRectTransform(rectTransform, offScreenLeft, centerScreen, halfDuration));

        // Scale up to 1.5x
        yield return StartCoroutine(ScaleRectTransform(rectTransform, Vector3.one, Vector3.one * 1.5f, scaleUpTime));

        // Scale back to original size
        yield return StartCoroutine(ScaleRectTransform(rectTransform, Vector3.one * 1.5f, Vector3.one, scaleUpTime));

        // Move from center to right
        yield return StartCoroutine(MoveRectTransform(rectTransform, centerScreen, offScreenRight, halfDuration));

        // Deactivate jumpText after the animation
        dialogText.SetActive(false);
    }

    public IEnumerator ShowMistakeFrame(float duration)
    {
        float halfDuration = duration / 2f;
        Color originalColor = mistakeImage.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Ensure the image starts fully transparent
        mistakeImage.color = transparentColor;
        mistakeImage.gameObject.SetActive(true);

        // Fade In
        float elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / halfDuration);
            mistakeImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Hold at full opacity for a moment if needed
        yield return new WaitForSeconds(0.1f);

        // Fade Out
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / halfDuration));
            mistakeImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }

    public void ShowEndScreen()
    {
        endScreen.SetActive(true);
        ScoreManager scoreManager = GetComponent<ScoreManager>();
        scoreManager.UpdateBestScore();
        scoreText.text = scoreManager.score.ToString();
        bestScoreText.text = "Best score: " + scoreManager.bestScore;
    }

    private IEnumerator MoveRectTransform(RectTransform rectTransform, Vector3 start, Vector3 end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector3.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = end;
    }

    private IEnumerator ScaleRectTransform(RectTransform rectTransform, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = endScale;
    }
}
