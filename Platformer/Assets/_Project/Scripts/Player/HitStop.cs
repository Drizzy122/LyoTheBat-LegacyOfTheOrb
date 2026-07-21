using System.Collections;
using UnityEngine;

namespace Platformer
{
    // Brief global time freeze on hit-confirm — the classic "crunch" of freeflow combat.
    // Bootstraps itself on first use so no scene wiring is needed. Timers and DOTween
    // both run on scaled time, so everything pauses together for free.
    public class HitStop : MonoBehaviour
    {
        static HitStop instance;

        Coroutine running;
        float ownTimeScale = -1f;

        static HitStop Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("HitStop");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<HitStop>();
                }
                return instance;
            }
        }

        public static void Play(float duration = 0.07f, float timeScale = 0.05f)
        {
            // Never fight the pause menu
            if (Mathf.Approximately(Time.timeScale, 0f)) return;
            Instance.PlayInternal(duration, timeScale);
        }

        void PlayInternal(float duration, float timeScale)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Freeze(duration, timeScale));
        }

        IEnumerator Freeze(float duration, float timeScale)
        {
            ownTimeScale = timeScale;
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);

            // Only restore if nobody else (pause menu, cutscene) changed it meanwhile
            if (Mathf.Approximately(Time.timeScale, ownTimeScale))
                Time.timeScale = 1f;

            ownTimeScale = -1f;
            running = null;
        }
    }
}
