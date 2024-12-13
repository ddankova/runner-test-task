using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BonusController : MonoBehaviour
{
    Coroutine scalingCoroutine = null;

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        // Start the infinite scaling animation
        scalingCoroutine = StartCoroutine(ScaleUpDown());
    }

    private IEnumerator ScaleUpDown()
    {
        Vector3 minScale = transform.localScale * 0.9f;
        Vector3 maxScale = transform.localScale * 1.1f;
        float duration = 0.3f;

        while (true)
        {
            // Scale up
            yield return StartCoroutine(ScaleObject(transform, minScale, maxScale, duration));
            // Scale down
            yield return StartCoroutine(ScaleObject(transform, maxScale, minScale, duration));
        }
    }

    private IEnumerator ScaleObject(Transform target, Vector3 startScale, Vector3 endScale, float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / time);
            target.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        target.localScale = endScale; // Ensure the final scale is accurate
    }

    private void OnDestroy()
    {
        // Stop the coroutine if the object is destroyed
        if (scalingCoroutine != null)
        {
            StopCoroutine(scalingCoroutine);
        }
    }
}
