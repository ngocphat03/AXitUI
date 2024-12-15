using UnityEngine;

namespace AXitUnityTemplate.UI.Sample.Scripts
{
    using System.Threading.Tasks;
    using AXitUnityTemplate.UI.Runtime.Scripts.Managers;
    using AXitUnityTemplate.UI.Sample.Scripts.Screens;

    public class TestManager : MonoBehaviour
    {
        public ScreenManager screenManager;
        
        
        private async void Start()
        {
            await Task.Delay(2000);
            
            this.screenManager.OpenScreen<DemoUIScreenPresenter>();
        }
    }
}
