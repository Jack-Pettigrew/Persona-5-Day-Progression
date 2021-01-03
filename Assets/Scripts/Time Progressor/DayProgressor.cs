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
        public int testSceneToLoad;
        private int sceneIndexToTransition;

        [SerializeField] private float completionWaitDelay = 3.0f;
        private bool animationFinished = false;

        // Coroutine
        private Coroutine progressionCoroutine = null;

        private void Awake()
        {
            if (!dayDirector)
            {
                Debug.LogError($"PlayableDirector has no reference on: {gameObject.name}");
            }

            sceneLoader = FindObjectOfType<SceneLoader>();
        }

        private void OnEnable()
        {
            SceneLoader.OnHasLoadedNewScene += StartTransitionToScene;
        }

        private void OnDisable()
        {
            SceneLoader.OnHasLoadedNewScene -= StartTransitionToScene;

        }

        private void Start()
        {
            StartProgression(testSceneToLoad);
        }

        private void StartProgression(int sceneIndex)
        {
            sceneIndexToTransition = sceneIndex;
            
            // Play while loading Async
            animationFinished = false;
            dayDirector.PlayDirector(delegate {
                animationFinished = true;
            });

            sceneLoader.ManualLoadSceneAsync(sceneIndexToTransition, SceneTransitionType.Fade);
        }

        private void StartTransitionToScene(UnityEngine.SceneManagement.Scene scene)
        {
            progressionCoroutine = StartCoroutine(TransitionCoroutine());
        }

        private IEnumerator TransitionCoroutine()
        {
            // wait for director to finish > ask to unload > complete
            yield return new WaitUntil(() => animationFinished == true);

            // Wait for optional timer
            yield return WaitTimer();

            sceneLoader.ManualTransitionToLoadedSceneAsync();

            progressionCoroutine = null;
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
