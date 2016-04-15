using System;
using System.Reflection;
using System.Windows.Controls;

namespace SexyReact.Views
{
    public class RxGrid<T> : Grid, IRxViewObject<T> 
        where T : IRxObject
    {
        private IRxViewObject<T> mixin;
        private bool isDisposed;

        public RxGrid()
        {
            mixin = new RxViewObject<T>(this);
        }

        public void Register(IDisposable disposable) => mixin.Register(disposable);
        public TValue Get<TValue>(PropertyInfo property) => mixin.Get<TValue>(property);
        public void Set<TValue>(PropertyInfo property, TValue value) => mixin.Set(property, value);
        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property) => mixin.ObserveProperty<TValue>(property);
        public IObservable<IPropertyChanging> Changing => mixin.Changing;
        public IObservable<IPropertyChanged> Changed => mixin.Changed;
        public IObservable<IPropertyChanged> GetChangedByProperty(PropertyInfo property) => mixin.GetChangedByProperty(property);
        public IObservable<IPropertyChanging> GetChangingByProperty(PropertyInfo property) => mixin.GetChangingByProperty(property);

        object IRxViewObject.Model
        {
            get { return Model; }
            set { Model = (T)value; }
        }

        public T Model
        {
            get { return mixin.Model; }
            set
            {
                mixin.Model = value;
                DataContext = value;
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                mixin.Dispose();
            }
        }
    }
}