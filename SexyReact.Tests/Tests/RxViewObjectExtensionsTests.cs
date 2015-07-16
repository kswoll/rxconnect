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
            view.BaseConnect();
        }

        private class TestLabel
        {
            public string Text { get; set; }
        }

        private class TestViewModel : RxObject
        {
            public string StringProperty { get; set; }
        }

        private class TestViewObject : IRxViewObject<TestViewModel>
        {
            private IRxViewObject<TestViewModel> mixin = new RxViewObject<TestViewModel>();
            private TestLabel testLabel = new TestLabel();

            public void BaseConnect()
            {
                var foo = this.Connect(x => x.testLabel.Text, From(x => x.StringProperty));
//                var foo = this.Connect(x => x.testLabel.Text).To(x => x.StringProperty);
//                return connect.To(x => x.StringProperty);
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
                return mixin.ObserveProperty<TValue>(property);
            }

            public TestViewModel Model
            {
                get { return mixin.Model; }
                set { mixin.Model = value; }
            }
        }
    }
}
