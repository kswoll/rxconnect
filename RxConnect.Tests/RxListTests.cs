using System;
using NUnit.Framework;
using System.Reactive.Linq;
using System.Reactive;

namespace RxConnect.Tests
{
    [TestFixture]
    public class RxListTests
    {
        [Test]
        public void Added()
        {
            var list = new RxList<string>();
            RxListItem<string> item = new RxListItem<string>();
            list.Added.Subscribe(x => item = x);
            
            list.Add("1");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("1", list[0]);
            list.Add("2");
            Assert.AreEqual(1, item.Index);
            Assert.AreEqual("2", item.Value);
            Assert.AreEqual("2", list[1]);
        }

        [Test]
        public void Removed()
        {
            var list = new RxList<string>(new[] { "1", "2" });
            RxListItem<string> item = new RxListItem<string>();
            list.Removed.Subscribe(x => item = x);
            
            list.Remove("1");
            Assert.AreEqual(item.Index, 0);
            Assert.AreEqual(item.Value, "1");

            list.Remove("2");
            Assert.AreEqual(item.Index, 0);
            Assert.AreEqual(item.Value, "2");

            list = new RxList<string>(new[] { "1", "2" });
            list.Removed.Subscribe(x => item = x);

            list.Remove("2");
            Assert.AreEqual(item.Index, 1);
            Assert.AreEqual(item.Value, "2");
            
            list.Remove("1");
            Assert.AreEqual(item.Index, 0);
            Assert.AreEqual(item.Value, "1");
        }
    }
}