using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public IEnumerator FadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        AudioSource audioSource = GetComponent<AudioSource>();

        float duration = 1f;
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, 0f, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}
