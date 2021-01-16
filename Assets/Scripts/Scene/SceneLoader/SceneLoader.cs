using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DD.Scene
{
    /// <summary>
    /// A Scene Loader helper class allowing for transitional scene loading.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        // TEST
        public int dayProgressionScene;

        // Loader Variables
        private SceneLoaderQueueItem currentLoaderItem = null;
        private Queue<SceneLoaderQueueItem> loaderQueue = new Queue<SceneLoaderQueueItem>();

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

        private bool IsSceneLoadingComplete
        {
            set;
            get;
        }
        public bool IsLoadedSceneActive
        {
            get { return LoadedScene == SceneManager.GetActiveScene(); }
        }

        private bool isLoaderReady = true;
        public bool IsLoaderReady
        {
            private set { isLoaderReady = value; }
            get { return isLoaderReady; }
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

        // THESE METHODS ARE FOR INTERFACING WITH THE SCENELOADER
        /// <summary>
        /// Automatically transition to loaded Scene when loading is complete.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        public void AutoLoadSceneAysnc(int buildIndex, SceneTransitionType transitionType)
        {
            if(!IsLoaderReady)  // Loader Busy Enqueue
            {
                loaderQueue.Enqueue(new SceneLoaderQueueItem(buildIndex, transitionType));
            }
            else // otherwise just get on with it
            {
                StartCoroutine(AutoAsyncLoad(buildIndex, transitionType));
            }
        }

        // THESE MEHTODS MANAGE WHEN LOADING AND UNLOADING HAPPENS (AUTOMATICALLY OR MANUALLY)

        private IEnumerator AutoAsyncLoad(int buildIndex, SceneTransitionType transitionType)
        {
            // Setup
            IsLoaderReady = false;
            IsSceneLoadingComplete = false;
            sceneTransitionType = transitionType;
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

            if(loaderQueue.Count > 0)
            {
                Debug.Log($"LoaderQueue: Loading next queued Scene.");
                currentLoaderItem = loaderQueue.Dequeue();
                yield return AutoAsyncLoad(currentLoaderItem.sceneBuildIndex, currentLoaderItem.sceneTransitionType);
            }
            else
            {
                currentLoaderItem = null;
                IsLoaderReady = true;
            }
        }

        // THESE METHODS ARE THE ACTUAL LOADING/UNLOADING LOGIC
        private IEnumerator LoadScene(int buildIndex)
        {
            // Load
            Debug.Log("Loading new scene...");
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;

            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);

            LoadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            IsSceneLoadingComplete = true;
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