using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SexyReact;
using SexyReact.Views;

namespace RxConnect.Tests.Tests
{
    [TestFixture]
    public class RxViewObjectExtensionsTests 
    {
        [Test]
        public void Connect()
        {
            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;
            view.BaseConnect();

            Assert.IsNull(view.testLabel.Text);
            model.StringProperty = "foo";
            Assert.AreEqual("foo", view.testLabel.Text);
        }

        private class TestLabel : RxObject
        {
            public string Text { get { return Get<string>(); } set { Set(value); } }
        }

        private class TestViewModel : RxObject
        {
            public string StringProperty { get { return Get<string>(); } set { Set(value); } }
        }

        private class TestViewObject : IRxViewObject<TestViewModel>
        {
            private IRxViewObject<TestViewModel> mixin = new RxViewObject<TestViewModel>();
            public readonly TestLabel testLabel = new TestLabel();

            public void BaseConnect()
            {
                this.Connect(x => x.testLabel.Text, From(x => x.StringProperty));
            }

            public void Dispose()
            {
            }

            public Expression<Func<TestViewModel, TValue>> From<TValue>(Expression<Func<TestViewModel, TValue>> property)
            {
                return mixin.From(property);
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

            public IObservable<TValue> ObserveProperty<TValue>(PropertyInfo property)
            {
                return mixin.ObserveProperty<TValue>(mixin.GetType().GetProperty(property.Name));
            }

            public TestViewModel Model
            {
                get { return mixin.Model; }
                set { mixin.Model = value; }
            }
        }
    }
}
