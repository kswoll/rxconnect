using System.Reactive.Concurrency;
using NUnit.Framework;

namespace SexyReact.Tests
{
    [SetUpFixture]
    public class RxTestSetup
    {
        [SetUp]
        public void SetUp()
        {
            Rx.UiScheduler = CurrentThreadScheduler.Instance;
        }
    }
}