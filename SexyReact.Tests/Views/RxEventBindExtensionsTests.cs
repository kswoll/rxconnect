using System;
using System.Reflection;
using NUnit.Framework;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    [TestFixture]
    public class RxEventBindExtensionsTests
    {
        [Test]
        public void Mate()
        {
            var view = new ViewClass();
            view.Bind(x => x.StringProperty)
                .Mate(view,
                    x => x.Value,
                    (x, value) => x.Value = value,
                    x => new EventHandler((sender, args) => x()),
                    (x, l) => x.ValueChanged += l, 
                    (x, l) => x.ValueChanged -= l);

            view.Model = new TestViewModel();
            view.Model.StringProperty = "foo";

            Assert.AreEqual("foo", view.Value);

            view.Value = "bar";
            Assert.AreEqual("bar", view.Model.StringProperty);
        }

        public class ViewClass : IRxViewObject<TestViewModel>
        {
            public event EventHandler ValueChanged;

            private IRxViewObject<TestViewModel> mixin;
            private bool isDisposed;
            private string value;

            public ViewClass()
            {
                mixin = new RxViewObject<TestViewModel>(this);
            }

            public string Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new EventArgs());
                    }
                }
            }

            public void Register(IDisposable disposable) => mixin.Register(disposable);
            public TValue Get<TValue>(PropertyInfo property) => mixin.Get<TValue>(property);
            public void Set<TValue>(PropertyInfo property, TValue value) => mixin.Set(property, value);
            public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property) => mixin.ObserveProperty<TValue>(property);
            public IObservable<IPropertyChanging> Changing => mixin.Changing;
            public IObservable<IPropertyChanged> Changed => mixin.Changed;

            object IRxViewObject.Model
            {
                get { return Model; }
                set { Model = (TestViewModel)value; }
            }

            public TestViewModel Model
            {
                get { return mixin.Model; }
                set
                {
                    mixin.Model = value;
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
}