namespace AXitUnityTemplate.UI.Sample.Scripts.Screens
{
    using System;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;
    using UnityEngine;

    public class DemoUIScreenPresenter : IScreenPresenter
    {
        public string                   ScreenId      { get; }
        public string                   ScreenPath    => "DemoUIScreenView";
        public EScreenStatus            EScreenStatus { get; }
        public Action<IScreenPresenter> OnCloseView   { get; set; }

        public void SetViewParent(Transform parent) { }

        public void SetView(IScreenView viewInstance, Action<IScreenPresenter> onClose = null) {}

        public Transform GetViewParent() { return default;}

        public Transform CurrentTransform { get; }

        public void OpenView() { }

        public void CloseView() { }

        public void Awake() { }

        public void OnEnable() {  }

        public void OnDisable() {  }

        public void OnDestroy() { }
    }
}