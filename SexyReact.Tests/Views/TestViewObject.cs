using System;
using System.Reflection;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    public class TestViewObject : IRxViewObject<TestViewModel>
    {
        private IRxViewObject<TestViewModel> mixin = new RxViewObject<TestViewModel>();
        public readonly TestLabel testLabel = new TestLabel();
        public TestSubViewObject subViewObject;
        public readonly NonRxTestLabel nonRxTestLabel = new NonRxTestLabel();

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

        public IObservable<IPropertyChanging> Changing
        {
            get { return mixin.Changing; }
        }
            
        public IObservable<IPropertyChanged> Changed
        {
            get { return mixin.Changed; }
        }

        public TValue Get<TValue>(PropertyInfo property)
        {
            return mixin.Get<TValue>(property);
        }

        public void Set<TValue>(PropertyInfo property, TValue value)
        {
            mixin.Set(property, value);
        }

        private static PropertyInfo modelProperty = typeof(RxViewObject<TestViewModel>).GetProperty("Model");

        public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property)
        {
            return mixin.ObserveProperty<TValue>(modelProperty);
        }

        public TestViewModel Model
        {
            get { return mixin.Model; }
            set { mixin.Model = value; }
        }
    }
}