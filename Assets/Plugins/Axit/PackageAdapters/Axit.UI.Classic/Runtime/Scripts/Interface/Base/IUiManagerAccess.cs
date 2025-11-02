namespace AxitUnityTemplate.UI.Classic
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Internal interface for ScreenManager access only.
    /// These methods should only be called by ScreenManager.
    /// </summary>
    internal interface IUiManagerAccess
    {
        /// <summary>
        /// Sets the view parent transform.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        void SetViewParent(Transform parent);

        /// <summary>
        /// Sets the view instance and initializes it.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        void SetView(object viewInstance);

        /// <summary>
        /// Opens the view.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        IEnumerator OpenView(Action onComplete = null);

        /// <summary>
        /// Closes the view.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        IEnumerator CloseView(Action onComplete = null);

        /// <summary>
        /// Sets the model for the presenter.
        /// IMPORTANT: This method should only be called from ScreenManager.
        /// </summary>
        void SetModel(object modelObject);
    }
}
