using System.Reflection;
using NUnit.Framework;
using SexyReact.Fody.Helpers;

namespace SexyReact.Tests
{
    [TestFixture]
    public class FodyTests
    {
        [Test]
        public void GetAndSet()
        {
            var obj = new TestFodyObject();
            obj.StringProperty = "foo";

            Assert.AreEqual("foo", obj.StringProperty);
        }
    }

    public class TestFodyObject : RxObject
    {
        [Rx]public string StringProperty { get; set; }
    }
}