using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DD.TimeProgression
{
    public class TimeProgressDirector : MonoBehaviour
    {
        // Director
        private PlayableDirector director;
        [SerializeField] private PlayableAsset introTimeline;
        [SerializeField] private PlayableAsset timeProgressTimeline;

        // Callback
        private Action onCompleteCallback = null;

        private void Awake()
        {
            director = GetComponent<PlayableDirector>();
            director.playOnAwake = false;

            onCompleteCallback = null;
        }

        public void PlayDirector(Action onCompleteCallback = null)
        {
            if (onCompleteCallback != null)
                this.onCompleteCallback = onCompleteCallback;

            director.stopped += PlayProgressionTimeline;
            director.Play(introTimeline);
        }

        private void PlayProgressionTimeline(PlayableDirector playableDirector)
        {
            playableDirector.stopped -= PlayProgressionTimeline;

            director.stopped += InvokeCompleteCallback;
            director.Play(timeProgressTimeline);
        }

        private void InvokeCompleteCallback(PlayableDirector playableDirector)
        {
            playableDirector.stopped -= InvokeCompleteCallback;

            onCompleteCallback?.Invoke();
        }
    }

}