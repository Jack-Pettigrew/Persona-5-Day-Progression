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

        // Current State
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

        public bool IsLoadedSceneActive
        {
            get { return LoadedScene == SceneManager.GetActiveScene(); }
        }

        #region IsLoaderReady
        private bool isLoaderReady = true;
        public bool IsLoaderReady
        {
            private set { isLoaderReady = value; }
            get { return isLoaderReady; }
        }
        #endregion

        // Loading Queue
        private SceneLoaderQueueItem currentLoaderItem = null;
        private Queue<SceneLoaderQueueItem> loaderQueue = new Queue<SceneLoaderQueueItem>();

        // Loading Operations
        private Coroutine loadingCoroutine = null;
        private AsyncOperation loadingAsyncOperation = null;
        private AsyncOperation unloadingAsyncOperation = null;

        // UI Transitioner
        [SerializeField] private SceneLoaderUITransitioner sceneLoaderUITransitioner = null;


        // TEST
        private void Start()
        {
            LoadSceneAysnc(dayProgressionScene, SceneTransitionType.Fade);
        }

        #region Auto Loading
        /// <summary>
        /// Instantly begin loading and transition to given Scene as soon as it's ready.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        public void LoadSceneAysnc(int buildIndex, SceneTransitionType transitionType)
        {
            if (!IsLoaderReady)  // Loader Busy Enqueue
            {
                loaderQueue.Enqueue(new SceneLoaderQueueItem(buildIndex, transitionType));
            }
            else // otherwise just get on with it
            {
                StartCoroutine(AutoAsyncLoad(buildIndex, transitionType));
            }
        }

        /// <summary>
        /// The logic for handling loading and transitioning to the new scene.
        /// </summary>
        /// <param name="buildIndex">The scene to load.</param>
        /// <param name="transitionType">The transition to use between scene loading.</param>
        /// <returns></returns>
        private IEnumerator AutoAsyncLoad(int buildIndex, SceneTransitionType transitionType)
        {
            // Setup
            IsLoaderReady = false;
            PreviousScene = LoadedScene;
            Debug.Log("Setup SceneLoader.");

            yield return sceneLoaderUITransitioner.StartTransitionEnter(transitionType);

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
            yield return sceneLoaderUITransitioner.StartTransitionExit(transitionType);

            // Finish Queue
            if (loaderQueue.Count > 0)
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
        #endregion

        #region Loading Logic
        /// <summary>
        /// Loads the Scene (Only Loads).
        /// </summary>
        /// <param name="buildIndex">The scene to load.</param>
        /// <returns></returns>
        private IEnumerator LoadScene(int buildIndex)
        {
            // Load
            Debug.Log("Loading new scene...");
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;

            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);

            Debug.Log("Loaded new scene.");
            LoadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
        }

        /// <summary>
        /// Transitions to the previously loaded scene (via LoadScene)
        /// </summary>
        /// <returns></returns>
        private IEnumerator UnloadActiveScene()
        {
            if (PreviousScene.handle != 0)
            {
                // Begin Unload
                Debug.Log("Unloading current scene...");
                unloadingAsyncOperation = SceneManager.UnloadSceneAsync(PreviousScene.buildIndex, UnloadSceneOptions.None);

                yield return WaitForCompleteOperation(1.0f, unloadingAsyncOperation);

                Debug.Log("Unloaded scene.");
            }
        }

        /// <summary>
        /// Wait until the given AsyncOperation has reached the completionThreshold.
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
        #endregion
    }
}