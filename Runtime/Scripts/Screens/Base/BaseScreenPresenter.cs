namespace AXitUnityTemplate.UI.Runtime.Scripts.Screens.Base
{
    using System;
    using UnityEngine;
    using System.Diagnostics;
    using Debug = UnityEngine.Debug;
    using AXitUnityTemplate.UI.Runtime.Scripts.Managers;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;

    public abstract class BaseScreenPresenter<TView> : IScreenPresenter where TView : BaseView
    {
        public string                   ScreenId      => this.GetType().Name;
        public EScreenStatus            EScreenStatus { get; private set; }
        public Action<IScreenPresenter> OnCloseView   { get; set; }
        public TView                    View          { get; private set; }

        public Transform CurrentTransform => (this.View as GameObject)?.transform
                                          ?? throw new Exception("View is not a game object");

        public abstract string ScreenPath { get; }

        public void SetViewParent(Transform parent) { this.View.transform.SetParent(parent); }

        public void SetView(IScreenView viewInstance, Action<IScreenPresenter> onClose = null)
        {
            this.View = viewInstance as TView;

            if (!this.View) throw new Exception("View is not of type TView");

            this.Awake();
        }

        public void OpenView()
        {
            if (this.EScreenStatus == EScreenStatus.Opened)
            {
                Debug.LogWarning("Screen is already opened");

                return;
            }

            this.View.ViewRoot.blocksRaycasts = true;
            this.EScreenStatus                = EScreenStatus.Opened;
            this.OnEnable();
            this.View.Open();
        }

        public void CloseView()
        {
            if (this.EScreenStatus == EScreenStatus.Closed)
            {
                Debug.LogWarning("Screen is already closed");

                return;
            }

            this.View.ViewRoot.blocksRaycasts = false;
            this.EScreenStatus                = EScreenStatus.Closed;
            this.View.Close(onCompleted: this.OnDisable);
        }

        public abstract void Awake();

        public abstract void OnEnable();

        public abstract void OnDisable();

        public abstract void OnDestroy();
    }
}