using System;
using NUnit.Framework;

namespace SexyReact.Tests
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

        [Test]
        public void ObservePropertyInitialValue()
        {
            var obj = new TestObject();
            string value = "error";
            obj.ObserveProperty(x => x.StringProperty).Subscribe(x => value = x);
            
            Assert.IsNull(value);
        }

        [Test]
        public void ObservePropertyInitialValueAlreadySet()
        {
            var obj = new TestObject();
            obj.StringProperty = "foo";
            string value = null;
            obj.ObserveProperty(x => x.StringProperty).Subscribe(x => value = x);
            
            Assert.AreEqual("foo", value);
        }

        [Test]
        public void ObservePropertyChanged()
        {
            var obj = new TestObject();
            string value = "error";
            obj.ObserveProperty(x => x.StringProperty).Subscribe(x => value = x);
            Assert.IsNull(value);
            
            obj.StringProperty = "foo";
            Assert.AreEqual("foo", value);
        }

        [Test]
        public void ObservePathChanged()
        {
            var obj = new ContainerObject();
            string value = "error";
            obj.ObserveProperty(x => x.Test.StringProperty).Subscribe(x => value = x);

            Assert.IsNull(value);

            var test = new TestObject();
            obj.Test = test;
            Assert.IsNull(value);

            obj.Test.StringProperty = "foo";
            Assert.AreEqual("foo", value);

            value = "error";
            obj.Test = null;

            value = "pass";
            test.StringProperty = "bar";
            Assert.AreEqual("pass", value);

            obj.Test = test;
            Assert.AreEqual("bar", value);
        }

        [Test]
        public void ObservePathChangedArray()
        {
            var obj = new ContainerObject();
            string value = "error";
            obj.ObserveProperty<ContainerObject, string>(new[] { typeof(ContainerObject).GetProperty("Test"), typeof(TestObject).GetProperty("StringProperty") }).Subscribe(x => value = x);

            Assert.IsNull(value);

            var test = new TestObject();
            obj.Test = test;
            Assert.IsNull(value);

            obj.Test.StringProperty = "foo";
            Assert.AreEqual("foo", value);

            value = "error";
            obj.Test = null;

            value = "pass";
            test.StringProperty = "bar";
            Assert.AreEqual("pass", value);

            obj.Test = test;
            Assert.AreEqual("bar", value);
        }

        public class TestObject : RxObject
        {
            public string StringProperty { get { return Get<string>(); } set { Set(value); } }
        }

        public class ContainerObject : RxObject
        {
            public TestObject Test { get { return Get<TestObject>(); } set { Set(value); } }
        }
    }
}