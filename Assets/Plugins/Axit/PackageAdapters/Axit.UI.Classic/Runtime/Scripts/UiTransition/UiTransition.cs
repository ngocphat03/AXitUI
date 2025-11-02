namespace AxitUnityTemplate.UI.Classic
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.EventSystems;

    public class UiTransition : MonoBehaviour
    {
        [SerializeField] private PlayableDirector   intro;
        [SerializeField] private PlayableDirector   outro;
        [SerializeField] private DirectorUpdateMode timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;

        private EventSystem eventSystem;
        private bool        isPlaying;

        private void Awake()
        {
            this.eventSystem = EventSystem.current;

            foreach (var director in new[] { this.intro, this.outro })
            {
                if (!director.playableAsset) break;
                director.time = 0f;
                director.Evaluate();
                director.timeUpdateMode = this.timeUpdateMode;
                director.playOnAwake    = false;
            }
        }

        public IEnumerator PlayIntroAnimation(Action onComplete = null)
        {
            this.SetInputLock(false);
            yield return this.PlayAnimation(this.intro, onComplete);
        }

        public IEnumerator PlayOutroAnimation(Action onComplete = null)
        {
            yield return this.PlayAnimation(this.outro, () =>
            {
                this.SetInputLock(true);
                onComplete?.Invoke();
            });
        }

        private IEnumerator PlayAnimation(PlayableDirector animationPlay, Action onComplete = null)
        {
            if (this.isPlaying || !animationPlay)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            this.isPlaying = true;

            animationPlay.Play();
            
            while (animationPlay.state == PlayState.Playing)
            {
                yield return null;
            }

            this.isPlaying = false;
            this.SetInputLock(true);
            onComplete?.Invoke();
        }

        private void SetInputLock(bool value)
        {
            if (!this.eventSystem)
            {
                Debug.LogWarning("EventSystem is not set, cannot lock input. In view:" + this.gameObject.name);
                return;
            }

            this.eventSystem.enabled = value;
        }
    }
}