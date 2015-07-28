using System;
using NUnit.Framework;
using SexyReact;

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

        [Test]
        public void ObserveProperty()
        {
            var obj = new TestFodyObject();
            string s = null;
            obj.ObserveProperty(x => x.StringProperty).Subscribe(x => s = x);
            obj.StringProperty = "foo";
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void StaticConstructor()
        {
            Assert.AreEqual("foo", TestFodyStaticConstructor.stringField);
            var testObject = new TestFodyStaticConstructor();
            testObject.StringProperty = "bar";
            Assert.AreEqual("bar", testObject.StringProperty);
        }

        [Test]
        public void EntireClass()
        {
            var obj = new RxClassObject();
            string s = null;
            obj.ObserveProperty(x => x.StringProperty).Subscribe(x => s = x);
            obj.StringProperty = "foo";
            Assert.AreEqual("foo", s);
        }
    }

    public class TestFodyObject : RxObject
    {
        [Rx]public string StringProperty { get; set; }
    }

    [Rx]
    public class RxClassObject : RxObject
    {
        public string StringProperty { get; set; }
    }

    public class TestFodyStaticConstructor : RxObject
    {
        public static string stringField;

        static TestFodyStaticConstructor()
        {
            stringField = "foo";
        }

        [Rx]public string StringProperty { get; set; }
    }
}