using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
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

        [Test]
        public async void ConnectRunsOnUIScheduler()
        {
            var originalThread = Thread.CurrentThread;
            var originalScheduler = Rx.UiScheduler;
            var completionSource = new TaskCompletionSource<Unit>();
            Rx.UiScheduler = NewThreadScheduler.Default;

            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;

            view.testLabel.TextSetHandler = () =>
            {
                var thread = Thread.CurrentThread;
                Assert.AreNotEqual(originalThread, thread);
                completionSource.SetResult(default(Unit));
            };
            view.Connect(view.testLabel, x => x.Text, x => x.StringProperty);

            await completionSource.Task;

            Rx.UiScheduler = originalScheduler;
        }
    }
}
