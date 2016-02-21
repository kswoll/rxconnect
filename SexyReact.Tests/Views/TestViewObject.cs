using System;
using System.Reflection;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    public class TestViewObject : IRxViewObject<TestViewModel>
    {
        private IRxViewObject<TestViewModel> mixin;
        public readonly TestLabel testLabel = new TestLabel();
        public TestSubViewObject subViewObject;
        public readonly NonRxTestLabel nonRxTestLabel = new NonRxTestLabel();

        public TestViewObject()
        {
            mixin = new RxViewObject<TestViewModel>(this);
        }

        object IRxViewObject.Model
        {
            get { return Model; }
            set { Model = (TestViewModel)value; }
        }

        public void Dispose()
        {
        }

        public void Register(IDisposable disposable)
        {
            throw new NotImplementedException();
        }

        public IObservable<IPropertyChanging> Changing => mixin.Changing;
        public IObservable<IPropertyChanged> Changed => mixin.Changed;
        public IObservable<IPropertyChanged> GetChangedByProperty(PropertyInfo property) => mixin.GetChangedByProperty(property);
        public IObservable<IPropertyChanging> GetChangingByProperty(PropertyInfo property) => mixin.GetChangingByProperty(property);
        public TValue Get<TValue>(PropertyInfo property) => mixin.Get<TValue>(property);
        public void Set<TValue>(PropertyInfo property, TValue value) => mixin.Set(property, value);
        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property) => mixin.ObserveProperty<TValue>(property);

        public TestViewModel Model
        {
            get { return mixin.Model; }
            set { mixin.Model = value; }
        }
    }
}