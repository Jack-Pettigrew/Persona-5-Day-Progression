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
        public int dayProgressionScene;

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

        private bool HasSceneFinishedLoading
        {
            set;
            get;
        }
        public bool IsLoadedSceneActive
        {
            get { return LoadedScene == SceneManager.GetActiveScene(); }
        }
        public bool IsLoaderFinished
        {
            private set;
            get;
        }

        private SceneTransitionType sceneTransitionType = SceneTransitionType.None;

        // Loading Operations
        private Coroutine loadingCoroutine = null;
        private AsyncOperation loadingAsyncOperation = null;
        private AsyncOperation unloadingAsyncOperation = null;

        // Loader Events
        public static Action<UnityEngine.SceneManagement.Scene> OnHasLoadedNewScene = delegate { };
        public static Action<UnityEngine.SceneManagement.Scene> OnHasUnloadedScene = delegate { };
        public static Action OnSceneLoaderFinished = delegate { };

        // Transition Events
        public static Action<SceneTransitionType> OnEnterTransition = delegate { };
        public static Action<SceneTransitionType> OnExitTransition = delegate { };

        // TEST
        private void Start()
        {
            AutoLoadSceneAysnc(dayProgressionScene, SceneTransitionType.None);
        }

        /// <summary>
        /// Load a Scene asynchronously - This is manual scene loading meaning that the scene will be loaded, but will not swap until TransitionSceneManual is called. This means it is possible to have two scenes loaded at a time before transition is called.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        public void ManualLoadSceneAsync(int buildIndex, SceneTransitionType transitionType)
        {
            sceneTransitionType = transitionType;
            loadingCoroutine = StartCoroutine(AsyncSceneLoadManual(buildIndex));
        }

        /// <summary>
        /// Manually transition to the new scene asynchronously. Will unload currently active scene and transition to the previously loaded new scene - use in conjunction with LoadSceneAsyncManual.
        /// </summary>
        public void ManualTransitionToLoadedSceneAsync()
        {
            StartCoroutine(AsyncUnloadSceneTransitionManual());
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
            HasSceneFinishedLoading = false;
            PreviousScene = LoadedScene;
            //OnEnterTransition.Invoke(sceneTransitionType);
            Debug.Log("Setup SceneLoader.");

            // Unload
            yield return UnloadActiveScene();

            // Load
            yield return LoadScene(buildIndex);

            // Activate new scene
            Debug.Log("Allowing scene to activate.");
            loadingAsyncOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => loadingAsyncOperation.isDone);
            SceneManager.SetActiveScene(LoadedScene);

            // Finish
            //OnExitTransition.Invoke(sceneTransitionType);
            IsLoaderFinished = true;
        }


        /// <summary>
        /// Asynchronous scene loading.
        /// </summary>
        /// <param name="buildIndex">sceneIndex of scene to load.</param>
        private IEnumerator AsyncSceneLoadManual(int buildIndex)
        {
            // Setup
            IsLoaderFinished = false;
            HasSceneFinishedLoading = false;
            PreviousScene = LoadedScene;
            //OnEnterTransition.Invoke(sceneTransitionType);

            // Load
            yield return LoadScene(buildIndex);
        }

        /// <summary>
        /// Asynchronous scene unloading.
        /// </summary>
        private IEnumerator AsyncUnloadSceneTransitionManual()
        {
            // Unload
            yield return UnloadActiveScene();

            // Activate new scene
            Debug.Log("Allowing scene to activate.");
            loadingAsyncOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => loadingAsyncOperation.isDone);
            SceneManager.SetActiveScene(LoadedScene);

            // Finish
            //OnExitTransition.Invoke(sceneTransitionType);
            IsLoaderFinished = true;
        }

        private IEnumerator LoadScene(int buildIndex)
        {
            // Load
            Debug.Log("Loading new scene...");
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;

            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);

            LoadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            HasSceneFinishedLoading = true;
            OnHasLoadedNewScene.Invoke(LoadedScene);
            Debug.Log("Loaded new scene.");
        }

        private IEnumerator UnloadActiveScene()
        {
            if (PreviousScene.handle != 0)
            {
                // Begin Unload
                Debug.Log("Unloading current scene...");
                unloadingAsyncOperation = SceneManager.UnloadSceneAsync(PreviousScene.buildIndex, UnloadSceneOptions.None);

                yield return WaitForCompleteOperation(1.0f, unloadingAsyncOperation);

                OnHasUnloadedScene.Invoke(PreviousScene);
                Debug.Log("Unloaded scene.");
            }
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