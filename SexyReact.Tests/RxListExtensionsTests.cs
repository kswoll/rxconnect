using NUnit.Framework;

namespace SexyReact.Tests
{
    [TestFixture]
    public class RxListExtensionsTests
    {
        [Test]
        public void DeriveWithSelectorInitialState()
        {
            var list = new RxList<string>("1", "2", "3");
            var derived = list.Derive(x => int.Parse(x));
            Assert.AreEqual(1, derived[0]);
            Assert.AreEqual(2, derived[1]);
            Assert.AreEqual(3, derived[2]);
        }

        [Test]
        public void DeriveWithSelectorAdd()
        {
            var list = new RxList<string>("1");
            var derived = list.Derive(x => int.Parse(x));
            list.Add("2");
            Assert.AreEqual(1, derived[0]);
            Assert.AreEqual(2, derived[1]);
        }

        [Test]
        public void DeriveWithSelectorInsertTwo()
        {
            var list = new RxList<string>("2");
            var derived = list.Derive(x => int.Parse(x));
            list.InsertRange(new[] { new RxListItem<string>(0, "1"), new RxListItem<string>(1, "3") });
            Assert.AreEqual(1, derived[0]);
            Assert.AreEqual(2, derived[1]);
            Assert.AreEqual(3, derived[2]);
        }

        [Test]
        public void DeriveWithSelectorRemoveTwo() 
        {
            var list = new RxList<string>("1", "2", "3");
            var derived = list.Derive(x => int.Parse(x));
            list.RemoveRange(new[] { "1", "3" });
            Assert.AreEqual(2, derived[0]);
        }

        [Test]
        public void DeriveWithSelectorModifyOne()
        {
            var list = new RxList<string>("1", "2", "3");
            var derived = list.Derive(x => int.Parse(x));
            list[1] = "4";
            Assert.AreEqual(1, derived[0]);
            Assert.AreEqual(4, derived[1]);
            Assert.AreEqual(3, derived[2]);
        }

        [Test]
        public void DeriveWithSelectorModifyTwo()
        {
            var list = new RxList<string>("1", "2", "3");
            var derived = list.Derive(x => int.Parse(x));
            list.ModifyRange(new[] { new RxListItem<string>(0, "4"), new RxListItem<string>(2, "5") });
            Assert.AreEqual(4, derived[0]);
            Assert.AreEqual(2, derived[1]);
            Assert.AreEqual(5, derived[2]);
        }

        [Test]
        public void DeriveWithSelectorMove()
        {
            var list = new RxList<string>("1", "2", "3");
            var derived = list.Derive(x => int.Parse(x));
            list.Move(0, 2);
            Assert.AreEqual(2, derived[0]);
            Assert.AreEqual(3, derived[1]);
            Assert.AreEqual(1, derived[2]);            
        }

        [Test]
        public void DeriveRemovesOnModified()
        {
            var list = new RxList<string>("1", "2", "3");
            int removed = -1;
            list.Derive(x => int.Parse(x), x => removed = x);
            list[0] = "5";
            Assert.AreEqual(1, removed);
        }

        [Test]
        public void DeriveRemovesOnRemoved()
        {
            var list = new RxList<string>("1", "2", "3");
            int removed = -1;
            list.Derive(x => int.Parse(x), x => removed = x);
            list.Remove("2");
            Assert.AreEqual(2, removed);
        }
    }
}