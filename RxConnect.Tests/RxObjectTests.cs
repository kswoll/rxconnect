using System;
using NUnit.Framework;

namespace RxConnect.Tests
{
    [TestFixture]
    public class RxObjectTests
    {
        [Test]
        public void GetReturnsWhatWasSet()
        {
            var obj = new TestObject();

            Assert.IsNull(obj.StringProperty);
            obj.StringProperty = "foo";
            Assert.AreEqual("foo", obj.StringProperty);
        }

        [Test]
        public void PropertyChanging()
        {
            var obj = new TestObject();
            var firstSubscription = obj.Changing.Subscribe(x =>
            {
                Assert.IsNull(x.OldValue);
                Assert.AreEqual("foo", x.NewValue);
            });
            obj.StringProperty = "foo";

            firstSubscription.Dispose();
            var secondSubscription = obj.Changing.Subscribe(x =>
            {
                Assert.AreEqual("foo", x.OldValue);
                Assert.AreEqual("bar", x.NewValue);
            });
            obj.StringProperty = "bar";
            secondSubscription.Dispose();
        }

        [Test]
        public void PropertyChangingOverrideNewValue()
        {
            var obj = new TestObject();
            obj.Changing.Subscribe(x => x.NewValue = x.NewValue + "ing");

            obj.StringProperty = "test";
            Assert.AreEqual("testing", obj.StringProperty);
        }

        public class TestObject : RxObject
        {
            public string StringProperty { get { return Get<string>(); } set { Set(value); } }
        }
    }
}