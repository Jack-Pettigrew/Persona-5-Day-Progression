using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DD.Scene;

namespace DD.Scene
{
    public class SceneUITransitioner : MonoBehaviour
    {
        [SerializeField] private float transitionSpeed = 1.0f;
        [SerializeField] private RawImage transitionImage;

        private Coroutine transitionCoroutine = null;

        private void OnEnable()
        {
            SceneLoader.OnEnterTransition += StartEnterTransition;
            SceneLoader.OnExitTransition += StartExitTransition;
        }

        private void OnDisable()
        {
            SceneLoader.OnEnterTransition -= StartEnterTransition;
            SceneLoader.OnExitTransition -= StartExitTransition;
        }

        private void StartEnterTransition(SceneTransitionType sceneTransitionType)
        {
            if(transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionEnter(sceneTransitionType));
        }

        private void StartExitTransition(SceneTransitionType sceneTransitionType)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionExit(sceneTransitionType));
        }

        private IEnumerator TransitionEnter(SceneTransitionType sceneTransitionType)
        {
            Color color = transitionImage.color;
            float progress = 0.0f;
            color.a = 0;

            while(progress < 1.0f)
            {
                progress += (Time.deltaTime * transitionSpeed);
                color.a = 255 * progress;
                transitionImage.color = color;

                yield return null;
            }

            transitionCoroutine = null;
        }

        private IEnumerator TransitionExit(SceneTransitionType sceneTransitionType)
        {
            Color color = transitionImage.color;
            float progress = 1.0f;
            color.a = 255;

            while (progress > 0.0f)
            {
                progress -= (Time.deltaTime * transitionSpeed);
                color.a = 255 * progress;
                transitionImage.color = color;

                yield return null;
            }

            transitionCoroutine = null;
        }
    } 
}
