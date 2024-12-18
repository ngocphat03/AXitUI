namespace AXitUnityTemplate.UI.Runtime.Scripts.Managers
{
    using System;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;

#if ZENJECT
    using Zenject;
#endif

    public class ScreenFactory
    {
#if ZENJECT
        private readonly DiContainer diContainer;
        public ScreenFactory(DiContainer diContainer) { this.diContainer = diContainer; }
#else
        public ScreenFactory()
#endif

        public IScreenPresenter CreateScreenPresenter(Type screenPresenterType)
        {
#if ZENJECT
            return this.diContainer.Instantiate(screenPresenterType) as IScreenPresenter;
#else
            return Activator.CreateInstance(screenPresenterType) as IScreenPresenter;
#endif
        }

        public T CreateScreenPresenter<T>() where T : IScreenPresenter
        {
#if ZENJECT
            return this.diContainer.Instantiate<T>();
#else
            return Activator.CreateInstance<T>();
#endif
        }
    }
}