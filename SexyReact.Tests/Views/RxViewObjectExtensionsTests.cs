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
        public void Bind()
        {
            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;

            view.Bind(x => x.StringProperty).To(view.testLabel, x => x.Text);

            Assert.IsNull(view.testLabel.Text);
            model.StringProperty = "foo";
            Assert.AreEqual("foo", view.testLabel.Text);
        }

        [Test]
        public void ConnectSubInitiallyNull()
        {
            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;

            view.Bind(x => x.StringProperty).To(view, x => x.subViewObject.testLabel.Text);

            var subObject = new TestSubViewObject();
            Assert.IsNull(subObject.testLabel.Text);
            model.StringProperty = "foo";
            Assert.IsNull(subObject.testLabel.Text);

            view.subViewObject = subObject;
            model.StringProperty = "foo2";
            Assert.AreEqual("foo2", subObject.testLabel.Text);
        }

        [Test]
        public void Biconnect()
        {
            var view = new TestViewObject();
            var model = new TestViewModel();
            view.Model = model;

            view.Bind(x => x.StringProperty).Mate(view.testLabel, x => x.Text);

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

            view.Bind(x => x.StringProperty).Mate(view.nonRxTestLabel);

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
            view.Bind(x => x.StringProperty).To(view.testLabel, x => x.Text);

            await completionSource.Task;

            Rx.UiScheduler = originalScheduler;
        }
/*

        [Test]
        public void ConnectPerformance()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < 10000; i++)
            {
                var viewObject = new TestViewObject();
                viewObject.Connect(x => viewObject.testLabel.Text = x, x => x.StringProperty);

//                var model = new TestViewModel();
//                model.StringProperty = "foo";
//                viewObject.Model = model;
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
        }

        [Test]
        public void ReplaySubjectPerformance()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Expression<Func<TestViewObject, string>> expression = x => x.Model.StringProperty;
            for (var i = 0; i < 10000; i++)
            {
                var rxObject = new TestViewObject();
                rxObject
                    .ObserveProperty(expression)
                    .SubscribeOnUiThread(_ => {});
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
        }
*/
    }
}
