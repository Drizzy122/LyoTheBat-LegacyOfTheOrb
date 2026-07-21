using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Platformer
{
    /// <summary>
    /// Drives the sonar-scan screen dim (detective-vision style): the world
    /// darkens while the scan window is open, then eases back to normal.
    /// Lives on a dedicated global Volume (weight 0 by default) whose profile
    /// holds the darkened look — this script only animates the weight.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class SonarVisionEffect : MonoBehaviour
    {
        public static SonarVisionEffect Instance { get; private set; }

        [SerializeField] Volume volume;

        [Tooltip("Seconds to fade the dim in when the scan fires.")]
        [SerializeField] float fadeInTime = 0.2f;

        [Tooltip("Seconds to ease back to normal at the end of the scan.")]
        [SerializeField] float fadeOutTime = 0.45f;

        Coroutine routine;

        void Awake()
        {
            if (volume == null) volume = GetComponent<Volume>();
            volume.weight = 0f;
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Dim the screen for the scan window. Total length ≈ duration.</summary>
        public void Play(float duration)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(PlayCo(duration));
        }

        IEnumerator PlayCo(float duration)
        {
            float hold = Mathf.Max(0f, duration - fadeInTime - fadeOutTime);

            yield return Fade(1f, fadeInTime);
            yield return new WaitForSeconds(hold);
            yield return Fade(0f, fadeOutTime);
            routine = null;
        }

        IEnumerator Fade(float target, float time)
        {
            float start = volume.weight;
            if (time <= 0f)
            {
                volume.weight = target;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                volume.weight = Mathf.Lerp(start, target, elapsed / time);
                yield return null;
            }
            volume.weight = target;
        }
    }
}
