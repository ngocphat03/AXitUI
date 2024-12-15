namespace AXitUnityTemplate.UI.Runtime.Scripts.Managers
{
    using System;
    using AXitUnityTemplate.UI.Runtime.Scripts.Interface;

    public class ScreenFactory
    {
        public T CreateScreenPresenter<T>() where T : IScreenPresenter
        {
            return Activator.CreateInstance<T>();
        }
    }
}