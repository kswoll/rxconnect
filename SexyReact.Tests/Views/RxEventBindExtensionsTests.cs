using System;
using NUnit.Framework;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    [TestFixture]
    public class RxEventBindExtensionsTests
    {
        [Test]
        public void Event()
        {
            var view = new ViewClass();
            view.Bind(x => x.StringProperty).Mate<TestViewModel, ViewClass, string, EventHandler>(view, x => x.Value, (x, l) => x.ValueChanged += l, (x, l) => x.ValueChanged -= l);

            view.Model = new ModelClass();


        }

        public class ViewClass : RxViewObject<TestViewModel>
        {
            public EventHandler ValueChanged;

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