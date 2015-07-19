using System;
using UIKit;
using SexyReact.Views;
using System.Reflection;
using Foundation;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace SexyReact.Ios
{
    [RxIos]
    public class RxViewController<T> : UIViewController, IRxViewObject<T>
        where T : IRxObject
    {
        private IRxViewObjectMixin<T> mixin = new RxViewObject<T>();

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

        public void Register(IDisposable disposable)
        {
            mixin.Register(disposable);
        }

        public TValue Get<TValue>(PropertyInfo property)
        {
            return mixin.Get<TValue>(property);
        }

        protected TValue Get<TValue>([CallerMemberName]string propertyName = null)
        {
            return mixin.Get<TValue>(propertyName);
        }

        public void Set<TValue>(PropertyInfo property, TValue value)
        {
            mixin.Set(property, value);
        }

        public void Set<TValue>(TValue newValue, [CallerMemberName]string propertyName = null)
        {
            mixin.Set(newValue, propertyName);
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

