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

        [Test]
        public void StaticConstructor()
        {
            Assert.AreEqual("foo", TestFodyStaticConstructor.stringField);
            var testObject = new TestFodyStaticConstructor();
            testObject.StringProperty = "bar";
            Assert.AreEqual("bar", testObject.StringProperty);
        }
    }

    public class TestFodyObject : RxObject
    {
        [Rx]public string StringProperty { get; set; }
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