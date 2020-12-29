using System.Collections;
using UnityEngine;

namespace DD.DayProgression
{
    public class DayProgressor : MonoBehaviour
    {
        [SerializeField]
        private DayProgressDirector director;

        // Coroutine
        private Coroutine completionCoroutine;

        private void Awake()
        {
            if(!director)
            {
                Debug.LogError($"PlayableDirector has no reference on: {gameObject.name}");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            StartProgression();
        }

        private void StartProgression()
        {
            // Tell SceneLoader to Async next scene

            // Play while loading Async
            director.PlayDirector();
        }

        private void CompleteProgression()
        {
            completionCoroutine = StartCoroutine(CompletionCoroutine());
        }

        private IEnumerator CompletionCoroutine()
        {
            // Wait until SceneLoader has loaded the next scene

            // Fade out
            
            // Reset everything/Remove this scene (dependant on project requirements)

            completionCoroutine = null;
            yield return null;
        }
    } 
}
