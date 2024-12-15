namespace AXitUnityTemplate.UI.Runtime.Scripts.Interface
{
    using System;
    using UnityEngine;

    public interface IScreenPresenter
    {
        public string                   ScreenId      { get; }
        public abstract string ScreenPath { get; }
        public EScreenStatus            EScreenStatus { get; }
        public Action<IScreenPresenter> OnCloseView   { get; set; }

        public void SetViewParent(Transform parent);

        public void SetView(IScreenView viewInstance, Action<IScreenPresenter> onClose = null);

        public Transform GetViewParent();

        public Transform CurrentTransform { get; }
        
        public void OpenView();

        public void CloseView();

        public void Awake();

        public void OnEnable();

        public void OnDisable();

        public void OnDestroy();
    }

    public interface IScreenPresenter<in TModel> : IScreenPresenter
    {
        public void OpenView(TModel model);
    }

    public enum EScreenStatus
    {
        Opened,
        Closed,
        Destroyed,
    }
}