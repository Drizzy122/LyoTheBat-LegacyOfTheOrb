using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Image
using System.Collections;
using System.Collections.Generic;

public class IntroSequence : MonoBehaviour
{
    [Header("Image Settings")]
    public Image splashImage;
    public CanvasGroup imageCanvasGroup;
    public float imageDuration = 3.0f; // How long to show the image

    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public CanvasGroup videoCanvasGroup;
    public List<VideoClip> introClips; 
    public string nextSceneName; 

    [Header("Transition Settings")]
    public float fadeDuration = 1.0f;

    private int videoIndex = 0;

    void Start()
    {
        StartCoroutine(FullSequence());
    }

    IEnumerator FullSequence()
    {
        // --- PART 1: THE IMAGE ---
        if (splashImage != null)
        {
            yield return StartCoroutine(Fade(imageCanvasGroup, 0, 1));
            yield return new WaitForSeconds(imageDuration);
            yield return StartCoroutine(Fade(imageCanvasGroup, 1, 0));
        }

        // --- PART 2: THE VIDEOS ---
        // Make the video group visible
        videoCanvasGroup.alpha = 1; 

        while (videoIndex < introClips.Count)
        {
            videoPlayer.clip = introClips[videoIndex];
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared) yield return null;

            videoPlayer.Play();

            // Wait until the video is almost done (minus fade time)
            float waitTime = (float)videoPlayer.length - fadeDuration;
            yield return new WaitForSeconds(Mathf.Max(0, waitTime));

            // If it's not the last video, we might want a quick dip to black
            if (videoIndex < introClips.Count - 1)
            {
                yield return StartCoroutine(Fade(videoCanvasGroup, 1, 0));
            }
            else
            {
                // Final video fade out
                yield return StartCoroutine(Fade(videoCanvasGroup, 1, 0));
            }

            videoIndex++;
        }

        // --- PART 3: LOAD SCENE ---
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(CanvasGroup cg, float start, float end)
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = end;
    }
}