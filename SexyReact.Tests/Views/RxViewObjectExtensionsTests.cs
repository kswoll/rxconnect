﻿using NUnit.Framework;
using SexyReact.Views;

namespace SexyReact.Tests.Views
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

            view.Connect(view.testLabel, x => x.Text, x => x.StringProperty);

            Assert.IsNull(view.testLabel.Text);
            model.StringProperty = "foo";
            Assert.AreEqual("foo", view.testLabel.Text);
        }

        [Test]
        public void Biconnect()
        {
            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;

            view.Biconnect(view.testLabel, x => x.Text, x => x.StringProperty);

            Assert.IsNull(model.StringProperty);
            view.testLabel.Text = "foo";
            Assert.AreEqual("foo", model.StringProperty);
        }

        [Test]
        public void BiconnectViewTargetNotRx()
        {
            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;

            view.Biconnect(view.nonRxTestLabel, x => x.StringProperty);

            Assert.IsNull(model.StringProperty);
            view.nonRxTestLabel.Text = "foo";
            Assert.AreEqual("foo", model.StringProperty);
        }
    }
}
