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
    }
}