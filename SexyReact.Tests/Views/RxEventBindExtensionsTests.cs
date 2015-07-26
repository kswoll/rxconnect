using System;
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

        public class ViewClass : RxViewObject<TestViewModel>
        {
            public event EventHandler ValueChanged;

            private string value;

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
        }
    }
}