using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DD.Scene
{
    /// <summary>
    /// An asynchronous Scene Loader helper class allowing for transitional scene loading.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        // TEST
        public int dayProgressionScene;

        // Loader State
        public int PreviousSceneIndex
        {
            private set;
            get;
        }

        public int LoadedSceneIndex
        {
            private set;
            get;
        }

        private bool isLoaderReady = true;
        public bool IsLoaderReady
        {
            private set { isLoaderReady = value; }
            get { return isLoaderReady; }
        }

        // Queue
        private SceneLoadRequest currentLoaderItem = null;
        private Queue<SceneLoadRequest> loaderQueue = new Queue<SceneLoadRequest>();

        // Operations
        private AsyncOperation loadingAsyncOperation = null;
        private AsyncOperation unloadingAsyncOperation = null;

        // UI Transitioner
        [SerializeField] private SceneLoaderUITransitioner sceneLoaderUITransitioner = null;

        // TEST
        private void Start()
        {
            LoadSceneAuto(dayProgressionScene, SceneTransitionType.Fade);
        }

        /// <summary>
        /// Instantly start loading a scene in the background. Use SceneLoadHandle to tell SceneLoader when to transition.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        /// <returns></returns>
        public SceneLoadHandle LoadSceneManual(int buildIndex, SceneTransitionType transitionType)
        {
            SceneLoadRequest newItem = new SceneLoadRequest(buildIndex, transitionType, SceneLoadType.Manual, new SceneLoadHandle());

            if (!IsLoaderReady)  // Loader Busy Enqueue
            {
                loaderQueue.Enqueue(newItem);
            }
            else // otherwise just get on with it
            {
                currentLoaderItem = newItem;
                StartCoroutine(ManualLoad());
            }

            return newItem.loadHandle;
        }

        /// <summary>
        /// Instantly begin loading and transition to given Scene as soon as it's ready.
        /// </summary>
        /// <param name="buildIndex">The scene buildIndex.</param>
        /// <param name="transitionType">The scene swap transition type.</param>
        public void LoadSceneAuto(int buildIndex, SceneTransitionType transitionType)
        {
            SceneLoadRequest newItem = new SceneLoadRequest(buildIndex, transitionType, SceneLoadType.Auto);

            if (!IsLoaderReady)  // Loader Busy Enqueue
            {
                loaderQueue.Enqueue(newItem);
            }
            else // otherwise just get on with it
            {
                currentLoaderItem = newItem;
                StartCoroutine(AutoLoad());
            }
        }

        private IEnumerator ManualLoad()
        {
            SetupLoad();

            yield return LoadNewScene(currentLoaderItem.sceneBuildIndex);

            currentLoaderItem.loadHandle.hasSceneLoaded = true;

            yield return new WaitUntil(() => currentLoaderItem.loadHandle.canSceneActivate == true);

            yield return sceneLoaderUITransitioner.StartTransitionEnter(currentLoaderItem.sceneTransitionType);
            
            /*  AsyncOperations will stall if allowsceneactivation = false.
             *  Work Around to avoid duplicate objects: 
             *      - Delete old scene GameObjects > activate new scene > unload old scene
             *      - Better workaround than LINQ? (LINQ creates unnecessary performance OH and GC)
             */
            SceneManager.GetSceneByBuildIndex(PreviousSceneIndex).GetRootGameObjects().ToList().ForEach((x) => Destroy(x));

            yield return ActivateLoadedScene();

            yield return UnloadScene(PreviousSceneIndex);

            yield return sceneLoaderUITransitioner.StartTransitionExit(currentLoaderItem.sceneTransitionType);

            HandleQueue();
        }
        
        /// <summary>
        /// The logic for handling loading and transitioning to the new scene.
        /// </summary>
        /// <param name="buildIndex">The scene to load.</param>
        /// <param name="transitionType">The transition to use between scene loading.</param>
        /// <returns></returns>
        private IEnumerator AutoLoad()
        {
            SetupLoad();

            yield return sceneLoaderUITransitioner.StartTransitionEnter(currentLoaderItem.sceneTransitionType);

            yield return UnloadScene(PreviousSceneIndex);

            yield return LoadNewScene(currentLoaderItem.sceneBuildIndex);

            yield return ActivateLoadedScene();

            yield return sceneLoaderUITransitioner.StartTransitionExit(currentLoaderItem.sceneTransitionType);

            HandleQueue();
        }

        #region Loading Logic
        /// <summary>
        /// Setups SceneLoader for loading.
        /// </summary>
        private void SetupLoad()
        {
            IsLoaderReady = false;
            PreviousSceneIndex = LoadedSceneIndex;
            Debug.Log("Setup SceneLoader.");
        }

        /// <summary>
        /// Scene load logic (Only Loads).
        /// </summary>
        /// <param name="buildIndex">The scene to load.</param>
        private IEnumerator LoadNewScene(int buildIndex)
        {
            // Load
            Debug.Log("Loading new scene...");
            loadingAsyncOperation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            loadingAsyncOperation.allowSceneActivation = false;

            yield return WaitForCompleteOperation(0.9f, loadingAsyncOperation);

            Debug.Log("Loaded new scene.");
            LoadedSceneIndex = buildIndex;
        }

        /// <summary>
        /// Scene unload logic (via LoadScene).
        /// </summary>
        private IEnumerator UnloadScene(int sceneBuildIndex)
        {
            if (sceneBuildIndex == 0 || !SceneManager.GetSceneByBuildIndex(sceneBuildIndex).isLoaded)               // sceneBuildIndex == 0 MIGHT PISS YOU OFF LATER
                yield break;

            Debug.Log($"Unloading current scene: {sceneBuildIndex}...");
            unloadingAsyncOperation = SceneManager.UnloadSceneAsync(sceneBuildIndex, UnloadSceneOptions.None);
            yield return WaitForCompleteOperation(1.0f, unloadingAsyncOperation);
            Debug.Log("Unloaded scene.");
        }

        /// <summary>
        /// Activates the newly loaded scene (the offical transition).
        /// </summary>
        private IEnumerator ActivateLoadedScene()
        {
            if (loadingAsyncOperation.allowSceneActivation)
                yield break;

            Debug.Log("Allowing scene to activate.");
            loadingAsyncOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => loadingAsyncOperation.isDone);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(LoadedSceneIndex));
        }

        /// <summary>
        /// Handles any remaining items in the Queue 
        /// </summary>
        private void HandleQueue()
        {
            if (loaderQueue.Count > 0)
            {
                Debug.Log($"LoaderQueue: Loading next queued Scene.");
                currentLoaderItem = loaderQueue.Dequeue();

                switch (currentLoaderItem.loadType)
                {
                    case SceneLoadType.Manual:
                        StartCoroutine(ManualLoad());

                        break;

                    default:
                        StartCoroutine(AutoLoad());

                        break;
                }
            }
            else
            {
                currentLoaderItem = null;
                IsLoaderReady = true;
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

public enum SceneLoadType { Auto, Manual }