using System;
using System.Collections.Generic;
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
        public void ObservePropertyIsDistinct()
        {
            var testObject = new TestObject();
            var containerObject = new ContainerObject();
            var list = new List<string>();
            containerObject.ObserveProperty(x => x.Test.StringProperty).Subscribe(x => list.Add(x));
            containerObject.Test = testObject;
            Assert.AreEqual(1, list.Count);
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

        [Test]
        public void PropertyForObservable()
        {
            var testObject = new TestObject();
            var stringPropertyObservable = testObject.ObserveProperty(x => x.StringProperty);

            var observableObject = new PropertyForObservableObject();
            observableObject.ObservableAsProperty(stringPropertyObservable, x => x.ProxyProperty);

            Assert.IsNull(observableObject.ProxyProperty);

            testObject.StringProperty = "foo";
            Assert.AreEqual("foo", observableObject.ProxyProperty);
        }

        [Test]
        public void ObserveTwoProperties()
        {
            var obj = new LotsOfProperties();
            Tuple<int, string> current = null;
            obj.ObserveProperties(x => x.IntProperty, x => x.StringProperty, (x, y) => Tuple.Create(x, y)).Subscribe(x => current = x);

            Assert.AreEqual(0, current.Item1);
            Assert.AreEqual(null, current.Item2);

            obj.IntProperty = 1;
            Assert.AreEqual(1, current.Item1);
            Assert.AreEqual(null, current.Item2);

            obj.StringProperty = "foo";
            Assert.AreEqual(1, current.Item1);
            Assert.AreEqual("foo", current.Item2);
        }

        [Test]
        public void ObserveThreeProperties()
        {
            var obj = new LotsOfProperties();
            Tuple<int, string, string> current = null;
            obj.ObserveProperties(x => x.IntProperty, x => x.StringProperty, x => x.TestObjectProperty.StringProperty, (x, y, z) => Tuple.Create(x, y, z)).Subscribe(x => current = x);

            Assert.AreEqual(0, current.Item1);
            Assert.AreEqual(null, current.Item2);
            Assert.AreEqual(null, current.Item3);

            obj.IntProperty = 1;
            Assert.AreEqual(1, current.Item1);
            Assert.AreEqual(null, current.Item2);
            Assert.AreEqual(null, current.Item3);

            obj.StringProperty = "foo";
            Assert.AreEqual(1, current.Item1);
            Assert.AreEqual("foo", current.Item2);
            Assert.AreEqual(null, current.Item3);

            obj.TestObjectProperty = new TestObject();
            obj.TestObjectProperty.StringProperty = "bar";
            Assert.AreEqual(1, current.Item1);
            Assert.AreEqual("foo", current.Item2);
            Assert.AreEqual("bar", current.Item3);
        }

        [Test]
        public void ObserveNonObservable() 
        {
            var container = new ContainerObject();
            string s = null;
            container.ObserveProperty(x => x.Test.NonObservableStringProperty).Subscribe(x =>
            {
                s = x;
            });
            var testObject = new TestObject();
            testObject.NonObservableStringProperty = "foo";
            container.Test = testObject;
            Assert.AreEqual("foo", s);
        }

        public class LotsOfProperties : RxObject
        {
            public int IntProperty { get { return Get<int>(); } set { Set(value); } }
            public string StringProperty { get { return Get<string>(); } set { Set(value); } }
            public TestObject TestObjectProperty { get { return Get<TestObject>(); } set { Set(value); } }
            public float FloatProperty { get { return Get<float>(); } set { Set(value); } }
        }

        public class TestObject : RxObject
        {
            public string StringProperty { get { return Get<string>(); } set { Set(value); } }
            public string NonObservableStringProperty { get; set; }
        }

        public class ContainerObject : RxObject
        {
            public TestObject Test { get { return Get<TestObject>(); } set { Set(value); } }
        }

        public class PropertyForObservableObject : RxObject
        {
            public string ProxyProperty { get { return Get<string>(); } }
        }
    }
}