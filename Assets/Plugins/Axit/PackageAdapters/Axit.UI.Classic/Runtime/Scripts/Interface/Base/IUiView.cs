namespace AxitUnityTemplate.UI.Classic
{
    using System;
    using System.Collections;
    using UnityEngine;

    public interface IUiView
    {
        public RectTransform RectTransform { get; }
        public event Action  OnViewReady;
        public event Action  OnOpen;
        public event Action  OnClose;
        public event Action  OnDestroy;

        public IEnumerator Open(Action onComplete = null);

        public IEnumerator Close(Action onComplete = null);

        public void DestroySelf();
    }
}