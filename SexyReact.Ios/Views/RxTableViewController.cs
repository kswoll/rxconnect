using System;
using UIKit;
using Foundation;
using System.Reflection;

namespace SexyReact.Views
{
    public class RxTableViewController<T> : UITableViewController, IRxViewObject<T>
        where T : IRxObject
    {
        private IRxViewObject<T> mixin;

        static RxTableViewController()
        {
            RxIos.RegisterDependency();
        }

        public RxTableViewController()
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewController(UITableViewStyle withStyle) : base(withStyle)
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewController(NSCoder coder) : base(coder)
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewController(NSObjectFlag t) : base(t)
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewController(IntPtr handle) : base(handle)
        {
            mixin = new RxViewObject<T>(this);
        }

        object IRxViewObject.Model
        {
            get { return Model; }
            set { Model = (T)value; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                mixin.Dispose();
            }
        }

        public void Register(IDisposable disposable) => mixin.Register(disposable);
        public TValue Get<TValue>(PropertyInfo property) => mixin.Get<TValue>(property);
        public void Set<TValue>(PropertyInfo property, TValue value) => mixin.Set(property, value);
        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property) => mixin.ObserveProperty<TValue>(property);
        public IObservable<IPropertyChanging> Changing => mixin.Changing;
        public IObservable<IPropertyChanged> Changed => mixin.Changed;
        public IObservable<IPropertyChanged> GetChangedByProperty(PropertyInfo property) => mixin.GetChangedByProperty(property);
        public IObservable<IPropertyChanging> GetChangingByProperty(PropertyInfo property) => mixin.GetChangingByProperty(property);

        public T Model
        {
            get { return mixin.Model; }
            set { mixin.Model = value; }
        }
    }
}
