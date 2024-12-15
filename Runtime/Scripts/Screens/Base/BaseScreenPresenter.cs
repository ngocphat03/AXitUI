// namespace AXitUnityTemplate.UI.Runtime.Scripts.Screens.Base
// {
//     using System;
//     using UnityEngine;
//     using UnityEngine.SceneManagement;
//     using AXitUnityTemplate.UI.Runtime.Scripts.Interface;
//
//     public abstract class BaseScreenPresenter<TView> : IScreenPresenter where TView : IScreenView
//     {
//         public string                   ScreenId      { get; private set; }
//         public EScreenStatus            EScreenStatus { get; protected set; } = EScreenStatus.Closed;
//         public Action<IScreenPresenter> OnCloseView   { get; set; }
//
//         public TView View;
//
//         public Transform GetViewParent() => this.View.RectTransform.parent;
//
//         public Transform CurrentTransform => this.View.RectTransform;
//
//         public void SetView(IScreenView viewInstance, Action<IScreenPresenter> onClose = null)
//         {
//             this.View             =  (TView)viewInstance;
//             this.ScreenId         =  $"{SceneManager.GetActiveScene().name}/{typeof(TView).Name}";
//             this.OnCloseView      =  onClose;
//             this.View.OnViewReady += this.Awake;
//         }
//
//         public void SetViewParent(Transform parent)
//         {
//             if (parent == null)
//             {
//                 Debug.LogError($"{parent.name} is null");
//
//                 return;
//             }
//
//             if (this.View.Equals(null)) return;
//             this.View.RectTransform.SetParent(parent);
//         }
//
//         public virtual void OpenViewAsync()
//         {
//             if (this.EScreenStatus == EScreenStatus.Opened) return;
//             this.EScreenStatus = EScreenStatus.Opened;
//             this.OnEnable();
//             this.View.Open();
//         }
//
//         public virtual void CloseViewAsync()
//         {
//             if (this.EScreenStatus == EScreenStatus.Closed) return;
//             this.EScreenStatus = EScreenStatus.Closed;
//             this.View.Close(onCompleted: () =>
//             {
//                 this.OnDisable();
//                 this.OnCloseView?.Invoke(this);
//             });
//         }
//
//         public void OnDestroy()
//         {
//             if (this.EScreenStatus == EScreenStatus.Destroyed) return;
//             this.EScreenStatus = EScreenStatus.Destroyed;
//
//             if (this.View.Equals(null)) return;
//             this.OnDisable();
//             this.View.DestroySelf();
//         }
//
//         public abstract void Awake();
//
//         public abstract void OnEnable();
//
//         public abstract void OnDisable();
//     }
//
//     public abstract class BaseScreenPresenter<TView, TModel> : BaseScreenPresenter<TView>, IScreenPresenter<TModel> where TView : IScreenView
//     {
//         protected TModel Model;
//
//         // public override async UniTask OpenView()
//         // {
//         //     if (this.Model != null)
//         //     {
//         //         await this.BindData(this.Model);
//         //     }
//         //     else
//         //     {
//         //         Debug.LogWarning($"{this.GetType().Name} don't have Model!!!");
//         //     }
//         //
//         //     await base.OpenViewAsync();
//         // }
//         //
//         // public virtual async UniTask OpenView(TModel model)
//         // {
//         //     this.Model = model != null ? model : this.Model;
//         //     await this.OpenView();
//         // }
//         //
//         // public sealed override UniTask BindData() { return UniTask.CompletedTask; }
//         //
//         // public abstract UniTask BindData(TModel screenModel);
//         public abstract void OpenView(TModel model);
//     }
// }