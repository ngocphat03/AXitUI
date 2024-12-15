namespace AXitUnityTemplate.AXitUI.Runtime.Scripts.Utilities
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScreenInfoAttribute : Attribute
    {
        public string AddressableScreenPath { get; }

        public ScreenInfoAttribute(string addressableScreenPath) { this.AddressableScreenPath = addressableScreenPath; }
    }
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PopupInfoAttribute : ScreenInfoAttribute
    {
        public bool IsOverlay             { get; }

        public PopupInfoAttribute(string addressableScreenPath, bool isOverlay = false) : base(addressableScreenPath)
        {
            this.IsOverlay             = isOverlay;
        }
    }
}