using System;
using System.Linq;
using NUnit.Framework;

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
        public void ItemAdded()
        {
            var list = new RxList<string>();
            string item = null;
            list.ItemAdded.Subscribe(x => item = x);
            
            list.Add("1");
            Assert.AreEqual("1", item);
            Assert.AreEqual("1", list[0]);
            list.Add("2");
            Assert.AreEqual("2", item);
            Assert.AreEqual("2", list[1]);
        }

        [Test]
        public void RangeAdded()
        {
            var list = new RxList<string>();
            RxListItem<string>[] items = null;
            list.RangeAdded.Subscribe(x => items = x.ToArray());
            
            list.AddRange(new[] { "1", "2" });
            Assert.AreEqual(0, items[0].Index);
            Assert.AreEqual("1", items[0].Value);
            Assert.AreEqual("1", list[0]);
            Assert.AreEqual(1, items[1].Index);
            Assert.AreEqual("2", items[1].Value);
            Assert.AreEqual("2", list[1]);
        }

        [Test]
        public void ItemsAdded()
        {
            var list = new RxList<string>();
            string[] items = null;
            list.ItemsAdded.Subscribe(x => items = x.ToArray());
            
            list.AddRange(new[] { "1", "2" });
            Assert.AreEqual("1", items[0]);
            Assert.AreEqual("1", list[0]);
            Assert.AreEqual("2", items[1]);
            Assert.AreEqual("2", list[1]);
        }

        [Test]
        public void RangeRemoved()
        {
            var list = new RxList<string>("1", "2", "3", "4");
            RxListItem<string>[] items = null;
            list.RangeRemoved.Subscribe(x => items = x.ToArray());
            
            list.RemoveRange(new[] { "2", "4" });
            Assert.AreEqual(1, items[0].Index);
            Assert.AreEqual("2", items[0].Value);
            Assert.AreEqual(3, items[1].Index);
            Assert.AreEqual("4", items[1].Value);
            Assert.AreEqual("1", list[0]);
            Assert.AreEqual("3", list[1]);
        }

        [Test]
        public void ItemsRemoved()
        {
            var list = new RxList<string>("1", "2", "3", "4");
            string[] items = null;
            list.ItemsRemoved.Subscribe(x => items = x.ToArray());
            
            list.RemoveRange(new[] { "2", "4" });
            Assert.AreEqual("2", items[0]);
            Assert.AreEqual("4", items[1]);
        }

        [Test]
        public void Removed()
        {
            var list = new RxList<string>("1", "2");
            var item = new RxListItem<string>();
            list.Removed.Subscribe(x => item = x);
            
            list.Remove("1");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("1", item.Value);

            list.Remove("2");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("2", item.Value);

            list = new RxList<string>("1", "2");
            list.Removed.Subscribe(x => item = x);

            list.Remove("2");
            Assert.AreEqual(1, item.Index);
            Assert.AreEqual("2", item.Value);
            
            list.Remove("1");
            Assert.AreEqual(0, item.Index);
            Assert.AreEqual("1", item.Value);
        }

        [Test]
        public void ItemRemoved()
        {
            var list = new RxList<string>("1", "2");
            string item = null;
            list.ItemRemoved.Subscribe(x => item = x);
            
            list.Remove("1");
            Assert.AreEqual("1", item);

            list.Remove("2");
            Assert.AreEqual("2", item);

            list = new RxList<string>("1", "2");
            list.ItemRemoved.Subscribe(x => item = x);

            list.Remove("2");
            Assert.AreEqual("2", item);
            
            list.Remove("1");
            Assert.AreEqual("1", item);
        }

        [Test]
        public void Modified()
        {
            var list = new RxList<string>("1a", "2a");

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
            var list = new RxList<string>("1a", "2a");
            
            var item = new RxListMovedItem<string>();
            list.Moved.Subscribe(x => item = x);

            list.Move(0, 1);
            Assert.AreEqual(0, item.FromIndex);
            Assert.AreEqual(1, item.ToIndex);
            Assert.AreEqual("1a", item.Value);
            Assert.AreEqual("2a", list[0]);
            Assert.AreEqual("1a", list[1]);
        }

        [Test]
        public void MovedItem()
        {
            var list = new RxList<string>("1a", "2a");
            
            var item = new RxListMovedItem<string>();
            list.Moved.Subscribe(x => item = x);

            list.Move(1, "1a");
            Assert.AreEqual(0, item.FromIndex);
            Assert.AreEqual(1, item.ToIndex);
            Assert.AreEqual("1a", item.Value);
            Assert.AreEqual("2a", list[0]);
            Assert.AreEqual("1a", list[1]);
        }
    }
}