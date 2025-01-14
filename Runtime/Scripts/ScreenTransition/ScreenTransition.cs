﻿namespace AXitUnityTemplate.UI.Runtime.Scripts.ScreenTransition
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.EventSystems;

    public class ScreenTransition : MonoBehaviour
    {
        [SerializeField] private PlayableDirector   intro;
        [SerializeField] private PlayableDirector   outro;
        [SerializeField] private bool               lockInput            = true;
        [SerializeField] private bool               playIntroFirstFrames = true;
        [SerializeField] private DirectorUpdateMode timeUpdateMode       = DirectorUpdateMode.UnscaledGameTime;

        private EventSystem eventSystem;
        private bool        isPlaying;
        private Action      onFinishAnimation;

        private void Awake()
        {
            this.eventSystem = EventSystem.current;

            // Set intro to play first frames
            if (this.playIntroFirstFrames && this.intro != null)
            {
                this.intro.time = 0;
                this.intro.Evaluate();
                this.intro.Pause();
            }

            // Check and add event for intro and outro
            foreach (var director in new[] { this.intro, this.outro })
            {
                if (!director.playableAsset) break;
                director.timeUpdateMode = this.timeUpdateMode;
                director.playOnAwake    = false;
            }
        }

        public void PlayIntroAnimation(Action onFinish = null)
        {
            this.SetInputLock(false);
            this.PlayAnimation(this.intro, onFinish);
        }

        public void PlayOutroAnimation(Action onFinish = null)
        {
            this.PlayAnimation(this.outro, () =>
            {
                onFinish?.Invoke();
                this.SetInputLock(true);
            });
        }

        private void PlayAnimation(PlayableDirector animationPlay, Action onFinish)
        {
            // Just play one times
            if (this.isPlaying) return;
            this.isPlaying = true;

            this.onFinishAnimation = onFinish;
            animationPlay?.Play();

            this.StartCoroutine(OnAnimationCompleteCoroutine());

            return;

            IEnumerator OnAnimationCompleteCoroutine()
            {
                // Wait until animation is done
                yield return new WaitUntil(() => animationPlay && animationPlay.state != PlayState.Playing);

                this.isPlaying = false;
                this.SetInputLock(true);
                this.onFinishAnimation?.Invoke();
                this.onFinishAnimation = null;
            }
        }

        private void SetInputLock(bool value)
        {
            // this.eventSystem.enabled = value;
        }
    }
}