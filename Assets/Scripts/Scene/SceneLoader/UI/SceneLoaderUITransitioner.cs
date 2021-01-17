using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DD.Scene
{
    public class SceneLoaderUITransitioner : MonoBehaviour
    {
        [SerializeField] private float transitionSpeed = 1.0f;
        [SerializeField] private RawImage transitionImage;

        private Coroutine transitionCoroutine = null;

        public IEnumerator StartTransitionEnter(SceneTransitionType sceneTransitionType)
        {
            switch (sceneTransitionType)
            {
                case SceneTransitionType.Cut:
                    yield return CutTransition(1.0f);
                    break;
                case SceneTransitionType.Fade:
                    yield return FadeTransition(1.0f);
                    break;
                default:
                    break;
            }
        }

        public IEnumerator StartTransitionExit(SceneTransitionType sceneTransitionType)
        {
            switch (sceneTransitionType)
            {
                case SceneTransitionType.Cut:
                    yield return CutTransition(0.0f);
                    break;
                case SceneTransitionType.Fade:
                    yield return FadeTransition(0.0f);
                    break;
                default:
                    break;
            }
        }

        private IEnumerator CutTransition(float targetAlpha)
        {
            transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, targetAlpha);
            return null;
        }

        /// <summary>
        /// Fades UI in/out depending on the order of parameters.
        /// </summary>
        /// <param name="targetAlpha">The target alpha.</param>
        private IEnumerator FadeTransition(float targetAlpha)
        {
            Color startColor = transitionImage.color;
            float progress = 0.0f;

            while (transitionImage.color.a != targetAlpha)
            {
                transitionImage.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp(Mathf.Lerp(startColor.a, targetAlpha, progress), 0, 255));
                progress += Time.deltaTime * transitionSpeed;

                yield return null;
            }

            transitionCoroutine = null;
        }
    }

    public enum SceneTransitionType { None, Cut, Fade };
}
