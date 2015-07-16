using NUnit.Framework;
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

            view.Biconnect(x => x.testLabel.Text, view.From(x => x.StringProperty));

            Assert.IsNull(model.StringProperty);
            view.testLabel.Text = "foo";
            Assert.AreEqual("foo", model.StringProperty);
        }


    }
}
