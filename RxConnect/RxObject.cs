using System;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RxConnect
{
    public class RxObject : IRxObject
    {
        private IStorageStrategy storageStrategy;
        private Subject<IPropertyChanged> changed = new Subject<IPropertyChanged>();
        private Subject<IPropertyChanging> changing = new Subject<IPropertyChanging>();
        private IObservePropertyStrategy observePropertyStrategy;
        private bool disposed;

        public RxObject()
        {
            storageStrategy = new DictionaryStorageStrategy();
            observePropertyStrategy = new DictionaryObservePropertyStrategy(this);
        }

        public RxObject(IStorageStrategy storageStrategy, IObservePropertyStrategy observePropertyStrategy)
        {
            this.storageStrategy = storageStrategy;
            this.observePropertyStrategy = observePropertyStrategy;
        }

        public IObservable<IPropertyChanging> Changing
        {
            get { return changing; }
        }

        public IObservable<IPropertyChanged> Changed
        {
            get { return changed; }
        }

        /// <summary>
        /// Gets the current value of the property as returned by the IStorageStrategy.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="property">The PropertyInfo corresponding to the property defined on your type for which
        /// you are attempting to get the current value.</param>
        /// <returns>The current value of the property</returns>
        protected TValue Get<TValue>(PropertyInfo property)
        {
            return storageStrategy.Retrieve<TValue>(property);
        }

        /// <summary>
        /// Gets the current value of the specified property.  This method should be called by your property getter 
        /// implementations.  This method uses reflection and string lookup of the PropertyInfo, so will inevitably 
        /// be slower than if you were to store the PropertyInfo yourself and pass it in directly to the other overload.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property.  You may omit this and the compiler will generate it for you. 
        /// (presuming, of course, that you are in fact calling it from your property getter)</param>
        /// <returns>The current value of the property.</returns>
        protected TValue Get<TValue>([CallerMemberName]string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("Property not found", "propertyName");

            return Get<TValue>(propertyInfo);
        }

        /// <summary>
        /// Sets the value of the specified property to newValue by delegating the call to the IStorageStrategy.  
        /// Furthermore, will propagate changing and changed notifications.  The new value may be modified by 
        /// subscribers to the changing notification.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="property">The PropertyInfo corresponding to the property defined on your type for which
        /// you are attempting to set the current value.</param>
        /// <param name="newValue">The new value of the property</param>
        protected void Set<TValue>(PropertyInfo property, TValue newValue)
        {
            TValue oldValue = storageStrategy.Retrieve<TValue>(property);
            
            var propertyChanging = new PropertyChanging<TValue>(property, oldValue, () => newValue, x => newValue = x);
            changing.OnNext(propertyChanging);
            
            storageStrategy.Store(property, newValue);

            var propertyChanged = new PropertyChanged<TValue>(property, oldValue, newValue);
            changed.OnNext(propertyChanged);

            observePropertyStrategy.OnNext(property, newValue);
        }

        /// <summary>
        /// Sets the current value of the specified property.  This method should be called by your property setter 
        /// implementations.  This method uses reflection and string lookup of the PropertyInfo, so will inevitably 
        /// be slower than if you were to store the PropertyInfo yourself and pass it in directly to the other overload.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="newValue">The new value of the property</param>
        /// <param name="propertyName">The name of the property.  You may omit this and the compiler will generate it for you. 
        /// (presuming, of course, that you are in fact calling it from your property setter)</param>
        protected void Set<TValue>(TValue newValue, [CallerMemberName]string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("Property not found", "propertyName");

            Set(propertyInfo, newValue);
        }

        /// <summary>
        /// Returns an observable that emits the current value of the specified property as it changes.  This observable
        /// will always immediately emit the current value of the property upon subscription.
        /// </summary>
        /// <typeparam name="TValue">The type of the property</typeparam>
        /// <param name="property">The property for which values should be observed.</param>
        /// <returns></returns>
        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property)
        {
            return observePropertyStrategy.ObservableForProperty<TValue>(property);
        }

        TValue IRxObject.Get<TValue>(PropertyInfo property)
        {
            return Get<TValue>(property);
        }

        void IRxObject.Set<TValue>(PropertyInfo property, TValue value)
        {
            Set(property, value);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                changed.Dispose();
                changing.Dispose();
                storageStrategy.Dispose();
                observePropertyStrategy.Dispose();
            }
        }
    }
}
