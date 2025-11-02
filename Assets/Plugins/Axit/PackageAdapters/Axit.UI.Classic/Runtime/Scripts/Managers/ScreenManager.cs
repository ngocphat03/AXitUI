namespace AxitUnityTemplate.UI.Classic
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if SCREEN_CLASSIC_ADDRESSABLE
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
#endif

    public sealed class ScreenManager : MonoBehaviour
    {
        [field: SerializeField] public Transform OpenedScreenParent { get; private set; }

        [field: SerializeField] public Transform ClosedScreenParent { get; private set; }
        
        [field: SerializeField] public bool AutoCreateScreenFactory { get; private set; } = false;

#if ZENJECT
#elif VCONTAINER
        [VContainer.Inject] private ScreenFactory screenFactory;
#else
        private readonly ScreenFactory screenFactory = new();
#endif

        public static ScreenManager Instance { get; private set; }

        private readonly Dictionary<Type, IScreenPresenter> screensPresenterLoaded = new(10);
        private readonly List<GameObject> uisViewLoaded = new(10);

        private IScreenPresenter CurrentScreen { get; set; }

        private readonly List<(IScreenPresenter screen, IScreenPresenter lastScreen)> historyScreen = new();

        private readonly Dictionary<Type, IPopupPresenter> popupsPresenterLoaded = new(10);

        public static Func<ScreenManager> Resolve = () => Instance;

#if VCONTAINER
        [VContainer.Inject]
        private void VContainerAwake()
        {
            this.Initialize();
        }
#else
        private void Awake()
        {
            this.Initialize();
        }
#endif
        
        private void Initialize()
        {
            if (ScreenManager.Instance != null && ScreenManager.Instance != this)
            {
                Debug.LogWarning("Multiple instances of ScreenManager detected. Destroying the new instance.");
                GameObject.Destroy(this.gameObject);
                return;
            }

            ScreenManager.Instance = this;

#if VCONTAINER
            // Check if the screen factory is set, if not and AutoCreateScreenFactory is true, create a new instance
            if (this.screenFactory == null && this.AutoCreateScreenFactory)
            {
                this.screenFactory = new ScreenFactory(null);
            }
#endif

            this.StartCoroutine(this.InitializeDelayed());
        }

        private IEnumerator InitializeDelayed()
        {
            yield return null;
            this.FindScreensInScene();
        }

        private void OnDestroy()
        {
            if (ScreenManager.Instance == this)
            {
                ScreenManager.Instance = null;
            }
            foreach (var screen in this.screensPresenterLoaded)
            {
                screen.Value.OnDestroy();
            }

            foreach (var popup in this.popupsPresenterLoaded)
            {
                popup.Value.OnDestroy();
            }
        }

        #region SCREEN
        
        public void OpenScreen<TPresenter, TModel>(TModel model = default, Action<TPresenter> onComplete = null) where TPresenter : IScreenPresenter where TModel : IScreenModel
        {
            this.StartCoroutine(this.OpenScreenCoroutine<TPresenter, TModel>(model, onComplete));
        }

        private IEnumerator OpenScreenCoroutine<TPresenter, TModel>(TModel model, Action<TPresenter> onComplete) where TPresenter : IScreenPresenter where TModel : IScreenModel
        {
            TPresenter presenter = default;
            yield return this.GetScreenCoroutine<TPresenter>(model, p => presenter = p);

            yield return this.CloseScreenCoroutine(this.CurrentScreen?.GetType(), openLastScreen: false, null);

            var screenOpen = this.historyScreen.Find(x => x.screen.GetType() == presenter.GetType());

            if (screenOpen == default)
            {
                screenOpen = (presenter, this.CurrentScreen);
                this.historyScreen.Add(screenOpen);
            }
            else
            {
                screenOpen.lastScreen = this.CurrentScreen;
            }
            ((IUiManagerAccess)presenter).SetViewParent(this.OpenedScreenParent);
            this.CurrentScreen = presenter;
            
            yield return ((IUiManagerAccess)this.CurrentScreen).OpenView(() => onComplete?.Invoke((TPresenter)this.CurrentScreen));
        }

        public void CloseScreen<TPresenter>(Action onComplete = null) where TPresenter : IScreenPresenter
        {
            this.StartCoroutine(this.CloseScreenCoroutine(typeof(TPresenter), openLastScreen: true, onComplete));
        }
        
        public void CloseScreen(Type typeScreenPresenter, bool openLastScreen = true, Action onComplete = null)
        {
            this.StartCoroutine(this.CloseScreenCoroutine(typeScreenPresenter, openLastScreen, onComplete));
        }

        private IEnumerator CloseScreenCoroutine(Type typeScreenPresenter, bool openLastScreen, Action onComplete)
        {
            if (typeScreenPresenter == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            if (!this.screensPresenterLoaded.TryGetValue(typeScreenPresenter, out var presenter))
            {
                Debug.LogError($"The {typeScreenPresenter.Name} screen does not exist");
                onComplete?.Invoke();
                yield break;
            }

            yield return ((IUiManagerAccess)presenter).CloseView(null);
            ((IUiManagerAccess)presenter).SetViewParent(this.ClosedScreenParent);
            
            if (!openLastScreen)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            var lastScreen = this.historyScreen.Find(x => x.screen.GetType() == typeScreenPresenter).lastScreen;

            if (lastScreen != null)
            {
                this.CurrentScreen = lastScreen;
                if (this.CurrentScreen == null)
                {
                    onComplete?.Invoke();
                    yield break;
                }

                ((IUiManagerAccess)this.CurrentScreen).SetViewParent(this.OpenedScreenParent);
                yield return ((IUiManagerAccess)this.CurrentScreen).OpenView(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void GetScreen<T>(IScreenModel model, Action<T> onComplete) where T : IScreenPresenter
        {
            this.StartCoroutine(this.GetScreenCoroutine<T>(model, onComplete));
        }

        private IEnumerator GetScreenCoroutine<T>(IScreenModel model, Action<T> onComplete) where T : IScreenPresenter
        {
            var screenType = typeof(T);

            if (this.screensPresenterLoaded.TryGetValue(screenType, out var screenPresenter))
            {
                ((IUiManagerAccess)screenPresenter).SetModel(model);
                onComplete?.Invoke((T)screenPresenter);
                yield break;
            }

            screenPresenter = this.screenFactory.CreateUiPresenter<T>() as IScreenPresenter;

            if (screenPresenter == null)
            {
                Debug.LogError($"The {screenType.Name} screen presenter does not exist");
                onComplete?.Invoke(default);
                yield break;
            }

#if SCREEN_CLASSIC_ADDRESSABLE
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(screenPresenter.ScreenPath);
            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load screen at path: {screenPresenter.ScreenPath}");
                onComplete?.Invoke(default);
                yield break;
            }
            
            var loadedAsset = handle.Result;
#else
            ResourceRequest request = Resources.LoadAsync<GameObject>(screenPresenter.ScreenPath);
            yield return request;
            
            var loadedAsset = request.asset as GameObject;
#endif

            if (loadedAsset == null)
            {
                Debug.LogError($"Failed to load screen at path: {screenPresenter.ScreenPath}");
                onComplete?.Invoke(default);
                yield break;
            }

            var viewObject = UnityEngine.Object.Instantiate(loadedAsset, this.OpenedScreenParent);

            if (!viewObject.TryGetComponent<IScreenView>(out var viewInstance))
            {
                Debug.LogError($"The {screenPresenter.ScreenPath} does not have a view component");
                onComplete?.Invoke(default);
                yield break;
            }

            this.uisViewLoaded.Add(viewObject);

            ((IUiManagerAccess)screenPresenter).SetModel(model);
            screenPresenter.OnCloseView += () => this.CloseScreen(screenPresenter.GetType(), openLastScreen: true, null);
            ((IUiManagerAccess)screenPresenter).SetView(viewInstance);
            this.screensPresenterLoaded[screenType] = screenPresenter;
            onComplete?.Invoke((T)screenPresenter);
        }
        
        #endregion

        #region POPUP
        
        public void OpenPopup<TPresenter, TModel>(TModel model = default, Action<TPresenter> onComplete = null) where TPresenter : IPopupPresenter where TModel : IPopupModel
        {
            this.StartCoroutine(this.OpenPopupCoroutine<TPresenter, TModel>(model, onComplete));
        }

        private IEnumerator OpenPopupCoroutine<TPresenter, TModel>(TModel model, Action<TPresenter> onComplete) where TPresenter : IPopupPresenter where TModel : IPopupModel
        {
            TPresenter presenter = default;
            yield return this.GetPopupCoroutine<TPresenter>(model, p => presenter = p);
            ((IUiManagerAccess)presenter).SetViewParent(this.OpenedScreenParent);
            yield return ((IUiManagerAccess)presenter).OpenView(() => onComplete?.Invoke(presenter));
        }

        public void ClosePopup<TPresenter>(Action onComplete = null) where TPresenter : IPopupPresenter
        {
            this.StartCoroutine(this.ClosePopupCoroutine(typeof(TPresenter), onComplete));
        }

        public void ClosePopup(Type typePresenter, Action onComplete = null)
        {
            this.StartCoroutine(this.ClosePopupCoroutine(typePresenter, onComplete));
        }

        private IEnumerator ClosePopupCoroutine(Type typePresenter, Action onComplete)
        {
            if (!this.popupsPresenterLoaded.TryGetValue(typePresenter, out var presenter))
            {
                Debug.LogError($"The {typePresenter.Name} popup does not exist");
                onComplete?.Invoke();
                yield break;
            }

            yield return ((IUiManagerAccess)presenter).CloseView(null);
            ((IUiManagerAccess)presenter).SetViewParent(this.ClosedScreenParent);
            onComplete?.Invoke();
        }

        public void GetPopup<T>(IPopupModel model, Action<T> onComplete) where T : IPopupPresenter
        {
            this.StartCoroutine(this.GetPopupCoroutine<T>(model, onComplete));
        }

        private IEnumerator GetPopupCoroutine<T>(IPopupModel model, Action<T> onComplete) where T : IPopupPresenter
        {
            var popupType = typeof(T);

            if (this.popupsPresenterLoaded.TryGetValue(popupType, out var popupPresenter))
            {
                ((IUiManagerAccess)popupPresenter).SetModel(model);
                onComplete?.Invoke((T)popupPresenter);
                yield break;
            }

            popupPresenter = this.screenFactory.CreateUiPresenter<T>() as IPopupPresenter;

            if (popupPresenter == null)
            {
                Debug.LogError($"The {popupType.Name} popup presenter does not exist");
                onComplete?.Invoke(default);
                yield break;
            }

#if SCREEN_CLASSIC_ADDRESSABLE
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(popupPresenter.PopupPath);
            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load popup at path: {popupPresenter.PopupPath}");
                onComplete?.Invoke(default);
                yield break;
            }
            
            var loadedAsset = handle.Result;
#else
            ResourceRequest request = Resources.LoadAsync<GameObject>(popupPresenter.PopupPath);
            yield return request;
            
            var loadedAsset = request.asset as GameObject;
#endif

            if (loadedAsset == null)
            {
                Debug.LogError($"Failed to load popup at path: {popupPresenter.PopupPath}");
                onComplete?.Invoke(default);
                yield break;
            }

            var viewObject = UnityEngine.Object.Instantiate(loadedAsset, this.OpenedScreenParent);

            if (!viewObject.TryGetComponent<IPopupView>(out var viewInstance))
            {
                Debug.LogError($"The {popupPresenter.PopupPath} does not have a view component");
                onComplete?.Invoke(default);
                yield break;
            }
            
            this.uisViewLoaded.Add(viewObject);

            ((IUiManagerAccess)popupPresenter).SetModel(model);
            popupPresenter.OnCloseView += () => this.ClosePopup(typeof(T), null);
            ((IUiManagerAccess)popupPresenter).SetView(viewInstance);
            this.popupsPresenterLoaded[popupType] = popupPresenter;
            onComplete?.Invoke((T)popupPresenter);
        }

        #endregion
        
        public void CloseAllScreensAndPopups()
        {
            foreach (var screen in this.screensPresenterLoaded)
            {
                this.CloseScreen(screen.Value.GetType(), openLastScreen: false, null);
            }

            foreach (var popup in this.popupsPresenterLoaded)
            {
                this.ClosePopup(popup.Value.GetType(), null);
            }

            this.CurrentScreen = null;
        }

        private void FindScreensInScene()
        {
            var allScreens = this.OpenedScreenParent.GetComponentsInChildren<IScreenView>(true)
                .Concat(this.ClosedScreenParent.GetComponentsInChildren<IScreenView>(true)).ToArray();

            var allPopups = this.OpenedScreenParent.GetComponentsInChildren<IPopupView>(true)
                .Concat(this.ClosedScreenParent.GetComponentsInChildren<IPopupView>(true)).ToArray();

            var loadedViews = new HashSet<GameObject>(this.uisViewLoaded);

            var firstScreenType = GetFirstScreenType();

            foreach (var view in allScreens)
            {
                if (loadedViews.Contains(((MonoBehaviour)view).gameObject)) continue;
                InitializeScreenView(view, firstScreenType);
            }

            foreach (var view in allPopups)
            {
                InitializePopupView(view);
            }
        }

        private Type GetFirstScreenType()
        {
            return this.OpenedScreenParent.childCount > 0
                ? this.OpenedScreenParent.GetChild(0)
                    .GetComponent<IScreenView>()?
                    .GetType()
                    .GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false)
                    ?.PresenterType
                : null;
        }

        private void InitializeScreenView(IScreenView view, Type firstScreenType)
        {
            var viewType = view.GetType();
            var attr = viewType.GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false);

            if (attr == null)
            {
                Debug.LogError($"The {viewType.Name} does not have a ViewInitInSceneAttribute, skipping initialization.");
                return;
            }

            var presenterType = attr.PresenterType;
            var presenterInstance = this.screenFactory.CreateUiPresenter(presenterType);

            if (presenterInstance is not IScreenPresenter presenter)
            {
                Debug.LogError(presenterInstance == null
                    ? $"The {presenterType.Name} screen presenter does not exist for view {viewType.Name}"
                    : $"The {presenterType.Name} screen presenter is not a valid type for view {viewType.Name}");
                return;
            }

            this.screensPresenterLoaded[presenterType] = presenter;
            ((IUiManagerAccess)presenter).SetModel(null);
            ((IUiManagerAccess)presenter).SetView(view);

            presenter.OnCloseView += () => this.CloseScreen(presenter.GetType(), openLastScreen: true, null);
            this.historyScreen.Add((presenter, null));

            if (firstScreenType != null && presenter.GetType() == firstScreenType)
            {
                this.StartCoroutine(((IUiManagerAccess)presenter).OpenView(null));
                this.CurrentScreen = presenter;
            }
            else
            {
                this.CloseScreen(presenter.GetType(), false, null);
            }
        }

        private void InitializePopupView(IPopupView view)
        {
            var viewType = view.GetType();
            var attr = viewType.GetCustomAttribute<ViewInitInSceneAttribute>(inherit: false);

            if (attr == null)
            {
                Debug.LogError($"The {viewType.Name} does not have a ViewInitInSceneAttribute, skipping initialization.");
                return;
            }

            var presenterType = attr.PresenterType;
            var presenterInstance = this.screenFactory.CreateUiPresenter(presenterType);

            if (presenterInstance is not IPopupPresenter presenter)
            {
                Debug.LogError(presenterInstance == null
                    ? $"The {presenterType.Name} popup presenter does not exist for view {viewType.Name}"
                    : $"The {presenterType.Name} popup presenter is not a valid type for view {viewType.Name}");
                return;
            }

            this.popupsPresenterLoaded[presenterType] = presenter;
            ((IUiManagerAccess)presenter).SetModel(null);
            ((IUiManagerAccess)presenter).SetView(view);

            presenter.OnCloseView += () => this.ClosePopup(presenter.GetType(), null);
            this.ClosePopup(presenter.GetType(), null);
        }
    }
}