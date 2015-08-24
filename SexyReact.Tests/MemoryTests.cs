using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace SexyReact.Tests
{
    [TestFixture]
    public class MemoryTests
    {
        [Test]
        public void OneMillionInstances()
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLine(process.PrivateMemorySize64);

            var list = new List<TestClass>();
            for (var i = 0; i < 1000 * 1000; i++)
            {
                var test = new TestClass();
//                test.Changed.Subscribe(x => test.IsModified = true);
                list.Add(test);
            }

            process.Refresh();
            Console.WriteLine(process.PrivateMemorySize64);
        }

//        [Rx]
        public class TestClass : RxObject
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime BirthDate { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public bool IsModified { get; set; }
        }
    }
}