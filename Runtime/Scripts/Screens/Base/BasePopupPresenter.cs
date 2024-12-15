// // namespace AXitUnityTemplate.ScreenTemplate.Scripts.Screens.Base
// // {
//      using System;
//      using AXitUnityTemplate.UI.Runtime.Scripts.Interface;
//      using UnityEngine;
//      using UnityEngine.SceneManagement;
// //
// public abstract class BasePopupPresenter<TView> : IScreenPresenter where TView : IScreenView
// {
//      public string                   ScreenId      { get; }
//      public EScreenStatus            EScreenStatus { get; }
//      public Action<IScreenPresenter> OnCloseView   { get; set; }
//
//      public void SetViewParent(Transform parent) { throw new NotImplementedException(); }
//
//      public void SetView(IScreenView viewInstance, Action<IScreenPresenter> onClose = null) { throw new NotImplementedException(); }
//
//      public Transform GetViewParent() { throw new NotImplementedException(); }
//
//      public Transform CurrentTransform { get; }
//
//      public void Awake() { throw new NotImplementedException(); }
//
//      public void OnEnable() { throw new NotImplementedException(); }
//
//      public void OnDisable() { throw new NotImplementedException(); }
//
//      public void OnDestroy() { throw new NotImplementedException(); }
// }
// //     {
// //         public string                   ScreenId     { get; private set; }
// //         public EScreenStatus             EScreenStatus { get; protected set; } = EScreenStatus.Closed;
// //         public Action<IScreenPresenter> OnCloseView  { get; set; }
// //
// //         public TView View;
// //
// //         public async void SetView(IScreenView viewInstance, Action<IScreenPresenter> onClose = null)
// //         {
// //             this.View        = (TView)viewInstance;
// //             this.ScreenId    = $"{SceneManager.GetActiveScene().name}/{typeof(TView).Name}";
// //             this.OnCloseView = onClose;
// //             if (!this.View.OnViewReady)
// //             {
// //                 await UniTask.WaitUntil(() => this.View.OnViewReady);
// //             }
// //
// //             this.OnViewReady();
// //         }
// //
// //         protected virtual void OnViewReady() { }
// //
// //         public void SetViewParent(Transform parent)
// //         {
// //             if (parent == null)
// //             {
// //                 Debug.LogWarning($"{parent.name} is null");
// //
// //                 return;
// //             }
// //
// //             if (this.View.Equals(null)) return;
// //             this.View.RectTransform.SetParent(parent);
// //         }
// //
// //         public Transform GetViewParent() => this.View.RectTransform.parent;
// //
// //         public Transform CurrentTransform => this.View.RectTransform;
// //
// //         public abstract UniTask BindData();
// //
// //         public virtual async UniTask OpenViewAsync()
// //         {
// //             await this.BindData();
// //
// //             if (this.EScreenStatus == EScreenStatus.Opened) return;
// //             this.EScreenStatus = EScreenStatus.Opened;
// //             await this.View.Open();
// //         }
// //
// //         public virtual async UniTask CloseViewAsync()
// //         {
// //             if (this.EScreenStatus == EScreenStatus.Closed) return;
// //             this.EScreenStatus = EScreenStatus.Closed;
// //             await this.View.Close();
// //             this.OnCloseView?.Invoke(this);
// //             this.Dispose();
// //         }
// //
// //         public virtual void Dispose() { }
// //
// //         public virtual async void CloseView() { await this.CloseViewAsync(); }
// //
// //         public virtual void HideView()
// //         {
// //             if (this.EScreenStatus == EScreenStatus.Hide) return;
// //             this.EScreenStatus = EScreenStatus.Hide;
// //             this.View.Hide();
// //             this.Dispose();
// //         }
// //
// //         public void OnDestroy()
// //         {
// //             if (this.EScreenStatus == EScreenStatus.Destroyed) return;
// //             this.EScreenStatus = EScreenStatus.Destroyed;
// //
// //             if (this.View.Equals(null)) return;
// //             this.Dispose();
// //             this.View.DestroySelf();
// //         }
// //
// //     }
// //
// //     public abstract class BasePopupPresenter<TView, TModel> : BasePopupPresenter<TView>, IScreenPresenter where TView : IScreenView
// //     {
// //         protected TModel Model;
// //
// //         public override async UniTask OpenViewAsync()
// //         {
// //             if (this.Model != null)
// //             {
// //                 await this.BindData(this.Model);
// //             }
// //             else
// //             {
// //                 Debug.LogWarning($"{this.GetType().Name} don't have Model!!!");
// //             }
// //
// //             await base.OpenViewAsync();
// //         }
// //
// //         public virtual async UniTask OpenView(TModel model)
// //         {
// //             this.Model = model != null ? model : this.Model;
// //             await this.OpenViewAsync();
// //         }
// //
// //         public sealed override UniTask BindData() { return UniTask.CompletedTask; }
// //
// //         public abstract UniTask BindData(TModel screenModel);
// //     }
// // }