namespace AXitUnityTemplate.AXitUI.Runtime.Scripts.Screens.Base
{
    using System;
    using UnityEngine;
    using AXitUnityTemplate.AXitUI.Runtime.Scripts.ScreenTransition;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;

    [RequireComponent(typeof(CanvasGroup), typeof(ScreenTransition))]
    public class BaseView : MonoBehaviour, IScreenView
    {
        [SerializeField] private CanvasGroup      viewRoot;
        [SerializeField] private ScreenTransition screenTransition;

        #region Public Properties

        public RectTransform RectTransform { get; private set; }
        public bool          blockRaycastHit = true;
        public event Action  OnViewReady;
        public event Action  OnOpen;
        public event Action  OnClose;
        public event Action  OnDestroy;

        #endregion

        protected virtual CanvasGroup      ViewRoot         { get => this.viewRoot;         set => this.viewRoot = value; }
        protected virtual ScreenTransition ScreenTransition { get => this.screenTransition; set => this.screenTransition = value; }

        private void Awake()
        {
            this.viewRoot         ??= this.GetComponent<CanvasGroup>();
            this.screenTransition ??= this.transform.GetComponent<ScreenTransition>();
            this.RectTransform    ??= this.GetComponent<RectTransform>();

            this.UpdateAlpha(0);
            this.OnViewReady?.Invoke();
        }

        public virtual void Open(Action onCompleted = null)
        {
            this.UpdateAlpha(1f);
            this.screenTransition.PlayIntroAnimation(() =>
            {
                this.OnOpen?.Invoke();
                onCompleted?.Invoke();
            });
        }

        public virtual void Close(Action onCompleted = null)
        {
            this.screenTransition.PlayOutroAnimation(() =>
            {
                this.UpdateAlpha(0);
                onCompleted?.Invoke();
                this.OnClose?.Invoke();
            });
        }

        public virtual void DestroySelf()
        {
            this.OnDestroy?.Invoke();
            UnityEngine.Object.Destroy(this.gameObject);
        }

        private void UpdateAlpha(float value)
        {
            if (this.viewRoot == null) return;
            this.ViewRoot.alpha          = value;
            this.ViewRoot.blocksRaycasts = this.blockRaycastHit && value >= 1;
        }
    }
}