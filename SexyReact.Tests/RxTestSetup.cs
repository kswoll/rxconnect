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
            RxApp.UiScheduler = CurrentThreadScheduler.Instance;
        }
    }
}