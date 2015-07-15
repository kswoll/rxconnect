using System;
using UIKit;
using RxConnect.Views;
using Foundation;
using System.Reflection;

namespace RxConnect.Ios
{
    public class RxTableViewController<T> : UITableViewController, IRxViewObject<T>
        where T : IRxObject
    {
        private IRxViewObject<T> mixin = new RxViewObject<T>();

        public RxTableViewController()
        {
        }

        public RxTableViewController(UITableViewStyle withStyle) : base(withStyle)
        {
        }

        public RxTableViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
        }

        public RxTableViewController(NSCoder coder) : base(coder)
        {
        }

        public RxTableViewController(NSObjectFlag t) : base(t)
        {
        }

        public RxTableViewController(IntPtr handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                mixin.Dispose();
            }
        }

        public TValue Get<TValue>(PropertyInfo property)
        {
            return mixin.Get<TValue>(property);
        }

        public void Set<TValue>(PropertyInfo property, TValue value)
        {
            mixin.Set(property, value);
        }

        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property)
        {
            return mixin.ObserveProperty<TValue>(property);
        }

        public IObservable<IPropertyChanging> Changing
        {
            get { return mixin.Changing; }
        }

        public IObservable<IPropertyChanged> Changed
        {
            get { return mixin.Changed; }
        }

        public T Model
        {
            get { return mixin.Model; }
            set { mixin.Model = value; }
        }
    }
}
