using System;
using UIKit;
using SexyReact.Views;
using System.Reflection;
using Foundation;

namespace SexyReact.Ios
{
    public class RxViewController<T> : UIViewController, IRxViewObject<T>
        where T : IRxObject
    {
        private IRxViewObject<T> mixin = new RxViewObject<T>();

        public RxViewController() 
        {
        }

        public RxViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
        }

        public RxViewController(NSCoder coder) : base(coder)
        {
        }

        public RxViewController(NSObjectFlag t) : base(t)
        {
        }

        public RxViewController(IntPtr handle) : base(handle)
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

