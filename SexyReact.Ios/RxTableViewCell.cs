using System;
using UIKit;
using SexyReact.Views;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SexyReact.Ios
{
    public class RxTableViewCell<T> : UITableViewCell, IRxViewObject<T>
        where T : IRxObject
    {
        public IRxCommand Command { get; set; }

        private IRxViewObjectMixin<T> mixin = new RxViewObject<T>();

        public RxTableViewCell()
        {
        }

        public RxTableViewCell(IntPtr handle)
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