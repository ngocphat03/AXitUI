namespace AXitUnityTemplate.UI.Runtime.Scripts.Managers
{
    using System;
    using System.Linq;
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;
    using AXitUnityTemplate.AssetLoader.Runtime.Interface;
    using AXitUnityTemplate.UI.Runtime.Scripts.Screens.Base;
    using Cysharp.Threading.Tasks;

#if ZENJECT
    using Zenject;

#else
    using AXitUnityTemplate.AssetLoader.Runtime.Utilities;
#endif

    public class ScreenManager : MonoBehaviour
    {
        [field: SerializeField] public Transform OpenedScreenParent { get; private set; }
        [field: SerializeField] public Transform ClosedScreenParent { get; private set; }

#if ZENJECT
        public ScreenFactory ScreenFactory { get; private set; }
        public static IAssetLoader  AssetLoader   { get; private set; }

        [Inject]
        public void Init(ScreenFactory screenFactory, IAssetLoader assetLoader)
        {
            this.ScreenFactory        = screenFactory;
            ScreenManager.AssetLoader = assetLoader;
            this.FindScreenInScene();
        }
#else
        public readonly ScreenFactory ScreenFactory = new();
        public static   IAssetLoader  AssetLoader => DependencyLocator.AssetLoader;
#endif

        private IScreenPresenter CurrentScreen { get; set; }

        private readonly Dictionary<Type, IScreenPresenter> screensPresenterLoaded = new(10);
        
        private List<IScreenPresenter> historyScreen = new();

        public void OpenScreen<T>(Action<T> onComplete = default) where T : IScreenPresenter
        {
            this.GetScreen<T>(presenter =>
            {
                if (presenter == null)
                {
                    Debug.LogError($"The {typeof(T).Name} screen does not exist");

                    return;
                }

                this.historyScreen.Add(presenter);
                this.StartCoroutine(CloseAndOpenScreen(presenter));
            });

            return;

            IEnumerator CloseAndOpenScreen(IScreenPresenter presenter)
            {
                // Close current screen
                this.CurrentScreen?.CloseView();
                this.CurrentScreen?.SetViewParent(this.ClosedScreenParent);

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

        public void CloseScreen<T>() where T : IScreenPresenter
        {
            if (!this.screensPresenterLoaded.TryGetValue(typeof(T), out var presenter))
            {
                Debug.LogError($"The {typeof(T).Name} screen does not exist");
                return;
            }
            
            if(presenter.EScreenStatus == EScreenStatus.Closed) return;
            presenter.CloseView();
            presenter.SetViewParent(this.ClosedScreenParent);
            
            // Check and open last screen
            this.historyScreen.Remove(presenter);
            var lastScreen = this.historyScreen.LastOrDefault();
            lastScreen?.OpenView();
        }

        public void OpenPopup<T>(Action<T> onComplete = default) where T : IScreenPresenter
        {
            this.GetScreen<T>(presenter =>
            {
                if (presenter == null)
                {
                    Debug.LogError($"The {typeof(T).Name} screen does not exist");

                    return;
                }

                this.StartCoroutine(CheckAndOpenPopup(presenter));
            });

            return;
            
            IEnumerator CheckAndOpenPopup(IScreenPresenter presenter)
            {
                // Open popup
                presenter.SetViewParent(this.OpenedScreenParent);
                presenter.OpenView();

                // Wait until new screen is opened
                while (this.CurrentScreen.EScreenStatus == EScreenStatus.Closed) yield return null;

                // Invoke onComplete callback
                onComplete?.Invoke((T)this.CurrentScreen);
            }
        }

        public void ClosePopup<T>() where T : IScreenPresenter
        {
            if (!this.screensPresenterLoaded.TryGetValue(typeof(T), out var presenter))
            {
                Debug.LogError($"The {typeof(T).Name} screen does not exist");
                return;
            }
            
            presenter.CloseView();
            presenter.SetViewParent(this.ClosedScreenParent);
        }

        public void GetScreen<T>(Action<T> onComplete) where T : IScreenPresenter
        {
            var screenType = typeof(T);

            // Check has screen presenter
            if (this.screensPresenterLoaded.TryGetValue(screenType, out var screenPresenter))
            {
                onComplete?.Invoke((T)screenPresenter);

                return;
            }
#if UNITASK
            InstantiateScreen(onComplete).Forget();
#else
            this.StartCoroutine(InstantiateScreen(onComplete));
#endif

            return;
#if UNITASK
            async UniTask InstantiateScreen(Action<T> callback)
            {
                screenPresenter = this.ScreenFactory.CreateScreenPresenter<T>();
                var view = await ScreenManager.AssetLoader.LoadAssetAsync<GameObject>(screenPresenter.ScreenPath).ToUniTask();

                // Get the view object
                var viewObject = UnityEngine.Object.Instantiate(view, this.OpenedScreenParent);

                // Check has view object
                if (!viewObject.TryGetComponent<IScreenView>(out var viewInstance))
                {
                    Debug.LogError($"The {screenPresenter.ScreenPath} does not have a view component");

                    return;
                }

                screenPresenter.SetView(viewInstance);

                // Cache the presenter
                this.screensPresenterLoaded[screenType] = screenPresenter;

                callback?.Invoke((T)screenPresenter);
            }

#else
            IEnumerator InstantiateScreen(Action<T> callback)
            {
                screenPresenter = this.ScreenFactory.CreateScreenPresenter<T>();
                var loadOperation = AssetLoader.LoadAssetAsync<GameObject>(screenPresenter.ScreenPath);

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

#endif
        }

        private void FindScreenInScene()
        {
            var allScreens = this.OpenedScreenParent.GetComponentsInChildren<BaseView>()
                                 .Concat(this.ClosedScreenParent.GetComponentsInChildren<BaseView>()).ToArray();
            foreach (var baseView in allScreens)
            {
                if (baseView is not IScreenDefaultInScene screenDefault) continue;

                var screenPresenter = this.ScreenFactory.CreateScreenPresenter(screenDefault.TypeScreenPresenter);

                // Set up view
                this.screensPresenterLoaded[screenDefault.TypeScreenPresenter] =  screenPresenter;
                baseView.OnViewReady                                           += () => screenPresenter.SetView(baseView);
            }
        }
    }
}