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
            var item = new RxListItem<string>();
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
            var item = new RxListItem<string>();
            list.Removed.Subscribe(x => item = x);
            
            list.Remove("1");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("1", item.Value);

            list.Remove("2");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("2", item.Value);

            list = new RxList<string>(new[] { "1", "2" });
            list.Removed.Subscribe(x => item = x);

            list.Remove("2");
            Assert.AreEqual(1, item.Index);
            Assert.AreEqual("2", item.Value);
            
            list.Remove("1");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("1", item.Value);
        }

        [Test]
        public void Modified()
        {
            var list = new RxList<string>(new[] { "1a", "2a" });

            var item = new RxListModifiedItem<string>();
            list.Modified.Subscribe(x => item = x);

            list[1] = "2b";
            Assert.AreEqual(1, item.Index);
            Assert.AreEqual("2a", item.OldValue);
            Assert.AreEqual("2b", item.NewValue);
        }

        [Test]
        public void Moved()
        {
            var list = new RxList<string>(new[] { "1a", "2a" });
            
            var item = new RxListMovedItem<string>();
            list.Moved.Subscribe(x => item = x);

            list.Move(0, 1);
            Assert.AreEqual(0, item.FromIndex);
            Assert.AreEqual(1, item.ToIndex);
            Assert.AreEqual("1a", item.Value);
            Assert.AreEqual("2a", list[0]);
            Assert.AreEqual("1a", list[1]);
        }
    }
}