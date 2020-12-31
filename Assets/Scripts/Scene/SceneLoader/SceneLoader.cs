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
        public int testLoadScene = 2;

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

        public bool IsLoaderFinished
        {
            private set;
            get;
        }
        public bool hasSceneFinishedLoading 
        { 
            private set; 
            get; 
        }
        public bool IsLoadedSceneActive
        {
            get { return LoadedScene == SceneManager.GetActiveScene(); }
        }

        private SceneTransitionType sceneTransitionType = SceneTransitionType.None;

        // Loading Operations
        private Coroutine loadingCoroutine = null;
        private AsyncOperation loadingAsyncOperation = null;
        private AsyncOperation unloadingAsyncOperation = null;

        // Event
        public static Action<SceneTransitionType> OnEnterTransition = delegate { };
        public static Action<SceneTransitionType> OnExitTransition = delegate { };

        // TEST
        private void Start()
        {
            AutoLoadSceneAysnc(testLoadScene, SceneTransitionType.Fade);
        }

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
            StartCoroutine(AsyncTransitionToLoadedScene());
        }

        /// <summary>
        /// Automatically transition to loaded Scene when loading is complete.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        public void AutoLoadSceneAysnc(int buildIndex, SceneTransitionType transitionType)
        {
            sceneTransitionType = transitionType;
            StartCoroutine(AutoAsyncLoad(buildIndex));
        }

        private IEnumerator AutoAsyncLoad(int buildIndex)
        {
            // Setup
            IsLoaderFinished = false;
            hasSceneFinishedLoading = false;
            PreviousScene = LoadedScene;
            OnEnterTransition.Invoke(sceneTransitionType);
            Debug.Log("Setup SceneLoader.");

            // Unload
            if (PreviousScene.handle != 0)
            {
                Debug.Log("Unloading current scene...");
                // Begin Unload
                unloadingAsyncOperation = SceneManager.UnloadSceneAsync(PreviousScene.buildIndex, UnloadSceneOptions.None);
                yield return WaitForCompleteOperation(1.0f, unloadingAsyncOperation);
                Debug.Log("Unloaded scene.");
            }

            // Load
            Debug.Log("Loading new scene...");
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;
            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);
            Debug.Log("Loaded new scene.");

            // Activate new scene
            Debug.Log("Allowing scene to activate.");
            loadingAsyncOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => loadingAsyncOperation.isDone);

            Debug.Log("Setting new Scene as the Active scene.");

            LoadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            SceneManager.SetActiveScene(LoadedScene);

            // Finish
            OnExitTransition.Invoke(sceneTransitionType);
            hasSceneFinishedLoading = true;
            IsLoaderFinished = true;

            //yield return AsyncSceneLoad(buildIndex);
            //yield return AsyncTransitionToLoadedScene();
        }


        /// <summary>
        /// Asynchronous scene loading.
        /// </summary>
        /// <param name="buildIndex">sceneIndex of scene to load.</param>
        private IEnumerator AsyncSceneLoad(int buildIndex)
        {
            IsLoaderFinished = false;
            hasSceneFinishedLoading = false;
            OnEnterTransition.Invoke(sceneTransitionType);

            PreviousScene = LoadedScene;

            // Begin Loading
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;
            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);

            LoadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            hasSceneFinishedLoading = true;
        }

        /// <summary>
        /// Asynchronous scene unloading.
        /// </summary>
        private IEnumerator AsyncTransitionToLoadedScene()
        {
            if(PreviousScene.handle != 0)
            {
                // Begin Unload
                unloadingAsyncOperation = SceneManager.UnloadSceneAsync(PreviousScene.buildIndex, UnloadSceneOptions.None);
                yield return WaitForCompleteOperation(1.0f, unloadingAsyncOperation);
            }

            // Activate loaded scene
            loadingAsyncOperation.allowSceneActivation = true;
            yield return null;

            SceneManager.SetActiveScene(LoadedScene);

            OnExitTransition.Invoke(sceneTransitionType);
            IsLoaderFinished = true;
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