using System.Collections;
using UnityEngine;
using DD.Scene;

namespace DD.DayProgression
{
    public class DayProgressor : MonoBehaviour
    {
        // Components
        [SerializeField] private DayProgressDirector dayDirector;
        private SceneLoader sceneLoader;

        // Variables
        public int nextScene;
        [SerializeField] private float completionWaitDelay = 3.0f;
        private SceneLoadHandle loadHandle = null;

        private void Awake()
        {
            if (!dayDirector)
            {
                Debug.LogError($"PlayableDirector has no reference on: {gameObject.name}");
            }

            sceneLoader = FindObjectOfType<SceneLoader>();
        }

        private void Start()
        {
            StartProgression();
        }

        private void StartProgression()
        {
            // Play while loading Async
            dayDirector.PlayDirector(delegate {
                StartCoroutine(TransitionCoroutine());
            });
        }

        private IEnumerator TransitionCoroutine()
        {
            // Wait for optional timer
            yield return WaitTimer();

            sceneLoader.LoadSceneAuto(nextScene, SceneTransitionType.Fade);
        }

        private IEnumerator WaitTimer()
        {
            float timer = completionWaitDelay;

            while(timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
    } 
}
