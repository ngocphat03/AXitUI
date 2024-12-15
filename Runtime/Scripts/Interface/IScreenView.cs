namespace AXitUnityTemplate.UI.Runtime.Scripts.Interface
{
    using System;
    using UnityEngine;

    public interface IScreenView
    {
        public RectTransform RectTransform { get; }
        public event Action  OnViewReady;
        public event Action  OnOpen;
        public event Action  OnClose;
        public event Action  OnDestroy;

        public void Open(Action onCompleted = null);

        public void Close(Action onCompleted = null);

        public void DestroySelf();
    }
}