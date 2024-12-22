namespace AXitUnityTemplate.UI.Runtime.Scripts.Screens.Base
{
    using System;
    using UnityEngine;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;
    using AXitUnityTemplate.UI.Runtime.Scripts.ScreenTransition;

    [RequireComponent(typeof(CanvasGroup), typeof(ScreenTransition))]
    public class BaseView : MonoBehaviour, IScreenView
    {
        #region Public Properties

        [field: SerializeField] public virtual CanvasGroup      ViewRoot         { get; protected set; }
        [field: SerializeField] public virtual ScreenTransition ScreenTransition { get; protected set; }

        public RectTransform RectTransform { get; private set; }
        public bool          blockRaycastHit = true;
        public event Action  OnViewReady;
        public event Action  OnOpen;
        public event Action  OnClose;
        public event Action  OnDestroy;

        #endregion

        private void Awake()
        {
            if (!this.ViewRoot) this.ViewRoot                 = this.GetComponent<CanvasGroup>();
            if (!this.ScreenTransition) this.ScreenTransition = this.transform.GetComponent<ScreenTransition>();
            if (!this.RectTransform) this.RectTransform       = this.GetComponent<RectTransform>();

            this.OnViewReady?.Invoke();
        }

        public virtual void Open(Action onCompleted = null)
        {
            this.UpdateAlpha(1f);
            this.ScreenTransition.PlayIntroAnimation(() =>
            {
                this.OnOpen?.Invoke();
                onCompleted?.Invoke();
            });
        }

        public virtual void Close(Action onCompleted = null)
        {
            this.ScreenTransition.PlayOutroAnimation(() =>
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
            if (!this.ViewRoot) return;
            this.ViewRoot.alpha          = value;
            this.ViewRoot.blocksRaycasts = this.blockRaycastHit && value >= 1;
        }
    }
}