using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DD.Scene
{
    public class SceneLoader : MonoBehaviour
    {
        public bool IsSceneLoaded { private set; get; }

        private void Start()
        {
            StartCoroutine(AsyncSceneTransition());
        }

        public IEnumerator AsyncSceneTransition()
        {
            AsyncOperation aOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            aOperation.allowSceneActivation = false;

            yield return WaitForAsync(aOperation);

            aOperation = SceneManager.UnloadSceneAsync(0, UnloadSceneOptions.None);

            yield return WaitForAsync(aOperation);

        }

        private IEnumerator WaitForAsync(AsyncOperation operation)
        {
            while (!operation.isDone)
            {
                Debug.Log(operation.progress);

                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}