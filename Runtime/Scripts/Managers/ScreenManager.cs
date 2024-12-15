namespace AXitUnityTemplate.UI.Runtime.Scripts.Managers
{
    using System;
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using AXitUnityTemplate.AssetLoader.Runtime.Interface;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;
    using AXitUnityTemplate.AssetLoader.Runtime.Utilities;

    public class ScreenManager : MonoBehaviour
    {
        [field: SerializeField] public Transform OpenedScreenParent { get; private set; }
        [field: SerializeField] public Transform ClosedScreenParent { get; private set; }

        public readonly ScreenFactory ScreenFactory = new();
        public static   IAssetLoader  AssetLoader => DependencyLocator.AssetLoader;

        private IScreenPresenter CurrentScreen { get; set; }

        private readonly Dictionary<Type, IScreenPresenter> screensPresenterLoaded = new(10);

        public void OpenScreen<T>(Action<T> onComplete = default) where T : IScreenPresenter
        {
            this.GetScreen<T>(presenter =>
            {
                if (presenter == null)
                {
                    Debug.LogError($"The {typeof(T).Name} screen does not exist");

                    return;
                }

                this.StartCoroutine(CloseAndOpenScreen(presenter));
            });

            return;

            IEnumerator CloseAndOpenScreen(IScreenPresenter presenter)
            {
                // Close current screen
                this.CloseCurrentScreen();

                // Wait until current screen is closed
                while (this.CurrentScreen?.EScreenStatus == EScreenStatus.Opened) yield return null;

                // Open new screen
                this.CurrentScreen = presenter;
                this.CurrentScreen.SetViewParent(this.OpenedScreenParent);
                this.CurrentScreen.OpenView();

                // Wait until new screen is opened
                while (this.CurrentScreen.EScreenStatus == EScreenStatus.Closed) yield return null;

                // Invoke onComplete callback
                onComplete?.Invoke((T)this.CurrentScreen);
            }
        }

        public void CloseScreen<T>() where T : IScreenPresenter { }

        public T OpenPopup<T>() where T : IScreenPresenter { return default; }

        public void ClosePopup<T>() where T : IScreenPresenter { }

        public void GetScreen<T>(Action<T> onComplete) where T : IScreenPresenter
        {
            var screenType = typeof(T);

            // Check has screen presenter
            if (this.screensPresenterLoaded.TryGetValue(screenType, out var screenPresenter))
            {
                onComplete?.Invoke((T)screenPresenter);

                return;
            }

            this.StartCoroutine(InstantiateScreen(onComplete));

            return;

            IEnumerator InstantiateScreen(Action<T> callback)
            {
                screenPresenter = this.ScreenFactory.CreateScreenPresenter<T>();
                var loadOperation = ScreenManager.AssetLoader.LoadAssetAsync<GameObject>(screenPresenter.ScreenPath);

                // Wait until asset is loaded
                while (!loadOperation.IsCompleted) yield return null;

                // Get the view object
                var viewObject = UnityEngine.Object.Instantiate(loadOperation.GetResult(), this.OpenedScreenParent);

                // Check has view object
                if (!viewObject.TryGetComponent<IScreenView>(out var viewInstance))
                {
                    Debug.LogError($"The {screenPresenter.ScreenPath} does not have a view component");

                    yield break;
                }

                screenPresenter.SetView(viewInstance);

                // Cache the presenter
                this.screensPresenterLoaded[screenType] = screenPresenter;

                callback?.Invoke((T)screenPresenter);
            }
        }

        public void CloseCurrentScreen() { this.CurrentScreen?.CloseView(); }
    }
}