using System;
using UIKit;
using System.Reflection;
using Foundation;

namespace SexyReact.Views
{
    public class RxTableViewCell<T> : UITableViewCell, IRxViewObject<T>
        where T : IRxObject
    {
        public IRxCommand Command { get; set; }

        private IRxViewObjectMixin<T> mixin;

        public RxTableViewCell() : base(UITableViewCellStyle.Default, typeof(T).FullName)
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewCell(IntPtr handle) : base(handle)
        {
            mixin = new RxViewObject<T>(this);
        }

        public RxTableViewCell(NSString reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
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