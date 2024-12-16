namespace AXitUnityTemplate.UI.Runtime.Scripts.Managers
{
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;

#if ZENJECT
    using Zenject;

#else
    using System;
#endif

    public class ScreenFactory
    {
#if ZENJECT
        private readonly DiContainer diContainer;
        public ScreenFactory(DiContainer diContainer) { this.diContainer = diContainer; }
#else
        public ScreenFactory()
#endif

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