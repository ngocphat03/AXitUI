#if ZENJECT
namespace AXitUnityTemplate.UI.Runtime.Scripts.Installer
{
    using Zenject;
    using UnityEngine;
    using AXitUnityTemplate.UI.Runtime.Scripts.Managers;

    public class UIZenjectInstaller : Installer<UIZenjectInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<ScreenFactory>().AsSingle().NonLazy();
            this.Container.Bind<ScreenManager>().FromInstance(Object.FindObjectOfType<ScreenManager>());
        }
    }
}
#endif
