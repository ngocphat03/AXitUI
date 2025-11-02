namespace AxitUnityTemplate.UI.Classic
{
    using System;
    using System.Collections;
    using UnityEngine;

    public abstract class BasePopupPresenter<TView, TModel> : IPopupPresenter, IUiManagerAccess where TView : BasePopupView where TModel : BasePopupModel, new()
    {
        private Action<IUiPresenter> onCloseView;

        public string UiId => this.GetType().Name;

        public EUiStatus EUiStatus { get; private set; }

        public Action OnCloseView { get; set; }

        public TView View { get; private set; }

        public TModel Model { get; private set; }

        public Transform CurrentTransform => (this.View as GameObject)?.transform
                                          ?? throw new Exception("View is not a game object");

        public abstract string PopupPath { get; }

        void IUiManagerAccess.SetViewParent(Transform parent)
        {
            this.View.transform.SetParent(parent);
        }

        void IUiManagerAccess.SetView(object viewInstance)
        {
            this.View = viewInstance as TView;

            if (!this.View) throw new Exception("View is not of type TView");

            this.View.Init();
            this.Awake();
        }

        IEnumerator IUiManagerAccess.OpenView(Action onComplete)
        {
            if (this.EUiStatus is EUiStatus.Opened or EUiStatus.Opening)
            {
                Debug.LogWarning("Popup is already opened");
                onComplete?.Invoke();
                yield break;
            }

            this.View.ViewRoot.blocksRaycasts = true;
            this.EUiStatus                    = EUiStatus.Opening;
            this.OnEnable();
            
            yield return this.View.Open(() =>
            {
                this.EUiStatus = EUiStatus.Opened;
                onComplete?.Invoke();
            });
        }

        IEnumerator IUiManagerAccess.CloseView(Action onComplete)
        {
            if (this.EUiStatus is EUiStatus.Closed or EUiStatus.Closing)
            {
                Debug.LogWarning("Popup is already closed");
                onComplete?.Invoke();
                yield break;
            }

            this.View.ViewRoot.blocksRaycasts = false;
            this.EUiStatus                    = EUiStatus.Closing;

            yield return this.View.Close(() =>
            {
                this.EUiStatus = EUiStatus.Closed;
                this.OnDisable();
                onComplete?.Invoke();
            });
        }

        void IUiManagerAccess.SetModel(object modelObject)
        {
            switch (modelObject)
            {
                case null:
                    if (this.Model != null) break;
                    
                    this.Model = new TModel();
                    break;
                case TModel tModel:
                    this.Model = tModel;
                    break;
                default:
                    Debug.LogError($"Model object is not of type {typeof(TModel).Name}. Expected: {typeof(TModel).Name}, Received: {modelObject.GetType().Name}");
                    return;
            }
        }
        
        public virtual void Awake(){}

        public virtual void OnEnable(){}

        public virtual void OnDisable(){}

        public virtual void OnDestroy(){}
    }
}