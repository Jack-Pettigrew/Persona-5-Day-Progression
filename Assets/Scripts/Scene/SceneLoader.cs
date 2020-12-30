using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DD.Scene
{
    /// <summary>
    /// A Scene Loader helper class allowing for transitional scene loading.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        // Loader Variables
        public UnityEngine.SceneManagement.Scene PreviousScene
        {
            private set;
            get;
        }
        public UnityEngine.SceneManagement.Scene LoadedScene
        {
            private set;
            get;
        }
        public bool IsSceneLoaded { 
            get { return loadingAsyncOperation?.progress >= 0.9f; }
        }
        public bool IsLoadedSceneActive
        {
            get { return loadingAsyncOperation?.progress >= 1.0f; }
        }

        private SceneTransitionType sceneTransitionType = SceneTransitionType.None;

        // Loading Operations
        private Coroutine loadingCoroutine = null;
        private AsyncOperation loadingAsyncOperation = null;
        private AsyncOperation unloadingAsyncOperation = null;

        // Event
        public static Action<SceneTransitionType> OnEnterTransition = delegate { };
        public static Action<SceneTransitionType> OnExitTransition = delegate { };

        /// <summary>
        /// Load a Scene asynchronously - This is manual scene loading meaning that the scene will be loaded, but will not swap until TransitionSceneManual is called.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        public void ManualLoadSceneAsync(int buildIndex, SceneTransitionType transitionType)
        {        
            sceneTransitionType = transitionType;
            loadingCoroutine = StartCoroutine(AsyncSceneLoad(buildIndex));
        }

        /// <summary>
        /// Manually transition to the new scene asynchronously - use in conjunction with LoadSceneAsyncManual.
        /// </summary>
        public void ManualTransitionToLoadedSceneAsync()
        {
            StartCoroutine(AsyncSceneUnloadOldScene());
        }

        /// <summary>
        /// Asynchronous scene loading.
        /// </summary>
        /// <param name="buildIndex">sceneIndex of scene to load.</param>
        private IEnumerator AsyncSceneLoad(int buildIndex)
        {
            OnEnterTransition.Invoke(sceneTransitionType);
            PreviousScene = SceneManager.GetActiveScene();

            // Begin Loading
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;
            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);

            LoadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
        }

        /// <summary>
        /// Asynchronous scene unloading.
        /// </summary>
        private IEnumerator AsyncSceneUnloadOldScene()
        {
            // Activate loaded scene
            loadingAsyncOperation.allowSceneActivation = true;

            // Begin Unload
            unloadingAsyncOperation = SceneManager.UnloadSceneAsync(PreviousScene.buildIndex, UnloadSceneOptions.None);
            yield return WaitForCompleteOperation(1.0f, unloadingAsyncOperation);

            SceneManager.SetActiveScene(LoadedScene);
            OnExitTransition.Invoke(sceneTransitionType);
        }

        /// <summary>
        /// Wait until the supplied AsyncOperation progress has reached the supplied completionThreshold.
        /// </summary>
        /// <param name="completionThreshold">The threshold to operation should reach.</param>
        /// <param name="operation">The AsyncOperation.</param>
        private IEnumerator WaitForCompleteOperation(float completionThreshold, AsyncOperation operation)
        {
            while (operation.progress < completionThreshold)
            {
                yield return null;
            }
        }
    }

    public enum SceneTransitionType { None, Fade };
}