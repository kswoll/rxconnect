﻿using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SexyReact.Tests
{
    [TestFixture]
    public class RxCommandTests
    {
        [Test]
        public async void CommandNoInputExecuteAsync()
        {
            string s = null;
            var command = RxCommand.CreateCommand(() => s = "foo");
            await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void CommandNoInputExecute()
        {
            string s = null;
            var command = RxCommand.CreateCommand(() => s = "foo");
            command.Execute();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public async void CommandWithInputExecuteAsync()
        {
            string s = null;
            var command = RxCommand.CreateCommand<string>(x => s = x);
            await command.ExecuteAsync("foo");
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void CommandWithInputExecute()
        {
            string s = null;
            var command = RxCommand.CreateCommand<string>(x => s = x);
            command.Execute("foo");
            Assert.AreEqual("foo", s);
        }

        [Test]
        public async void FunctionNoInputExecuteAsync()
        {
            string observed = null;
            var command = RxCommand.CreateFunction(() => "foo");
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public void FunctionNoInputExecute()
        {
            string observed = null;
            var command = RxCommand.CreateFunction(() => "foo");
            command.Subscribe(x => observed = x);
            var s = command.Execute();
            Assert.AreEqual("foo", s);            
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public async void FunctionWithInputExecuteAsync()
        {
            string observed = null;
            var command = RxFunction<string>.CreateFunction(x => x);
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync("foo");
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public void FunctionWithInputExecute()
        {
            string observed = null;
            var command = RxFunction<string>.CreateFunction(x => x);
            command.Subscribe(x => observed = x);
            var s = command.Execute("foo");
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        private async Task<string> GetFoo()
        {
            await Task.Delay(0);
            return "foo";
        }

        [Test]
        public async void AsyncCommandNoInputExecuteAsync()
        {
            string s = null;
            var command = RxCommand.CreateCommand(async () => s = await GetFoo());
            await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void AsyncCommandNoInputExecute()
        {
            string s = null;
            var command = RxCommand.CreateCommand(async () => s = await GetFoo());
            command.Execute();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public async void AsyncCommandWithInputExecuteAsync()
        {
            string s = null;
            var command = RxCommand.CreateCommand<string>(async x => s = x + await GetFoo());
            await command.ExecuteAsync("foo");
            Assert.AreEqual("foofoo", s);
        }

        [Test]
        public void AsyncCommandWithInputExecute()
        {
            string s = null;
            var command = RxCommand.CreateCommand<string>(async x => s = x + await GetFoo());
            command.Execute("foo");
            Assert.AreEqual("foofoo", s);
        }

        [Test]
        public async void AsyncFunctionNoInputExecuteAsync()
        {
            string observed = null;
            var command = RxCommand.CreateFunction(async () => await GetFoo());
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public void AsyncFunctionNoInputExecute()
        {
            string observed = null;
            var command = RxCommand.CreateFunction(async () => await GetFoo());
            command.Subscribe(x => observed = x);
            var s = command.Execute();
            Assert.AreEqual("foo", s);            
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public async void AsyncFunctionWithInputExecuteAsync()
        {
            string observed = null;
            var command = RxFunction<string>.CreateFunction(async x => x + await GetFoo());
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync("foo");
            Assert.AreEqual("foofoo", s);
            Assert.AreEqual("foofoo", observed);
        }

        [Test]
        public void AsyncFunctionWithInputExecute()
        {
            string observed = null;
            var command = RxFunction<string>.CreateFunction(async x => x + await GetFoo());
            command.Subscribe(x => observed = x);
            var s = command.Execute("foo");
            Assert.AreEqual("foofoo", s);
            Assert.AreEqual("foofoo", observed);
        }

        [Test]
        public async void CombineTwoNoInputCommands()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxCommand.CreateCommand(() =>
            {
                Assert.AreEqual("foo", a);
                b = "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            await combinedCommand.ExecuteAsync();

            Assert.AreEqual("bar", b);
        }

        [Test]
        public async void CombineTwoCommandsFirstNoInputSecondWithInput()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxCommand.CreateCommand<string>(x =>
            {
                Assert.AreEqual("foo", a);
                b = x;
            });
            var combinedCommand = commandA.Combine(commandB);
            await combinedCommand.ExecuteAsync("bar");

            Assert.AreEqual("bar", b);
        }

        [Test]
        public async void CombineTwoCommandsFirstWithInputSecondWithNoInput()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand<string>(x =>
            {
                Assert.IsNull(b);
                a = x;
            });
            var commandB = RxCommand.CreateCommand(() =>
            {
                Assert.AreEqual("foo", a);
                b = "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            await combinedCommand.ExecuteAsync("foo");

            Assert.AreEqual("bar", b);
        }

        [Test]
        public async void CombineTwoCommandsBothWithInput()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand<string>(x =>
            {
                Assert.IsNull(b);
                a = x;
            });
            var commandB = RxCommand.CreateCommand<string>(x =>
            {
                Assert.AreEqual("foo", a);
                b = x;
            });
            var combinedCommand = commandA.Combine(commandB);
            await combinedCommand.ExecuteAsync("foo");

            Assert.AreEqual("foo", b);
        }

        [Test]
        public async void CombineCommandWithFunctionNoInput()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxCommand.CreateFunction(() =>
            {
                Assert.AreEqual("foo", a);
                b = "done";
                return "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            var result = await combinedCommand.ExecuteAsync();

            Assert.AreEqual("bar", result);
        }

        [Test]
        public async void CombineCommandWithFunctionInputFirstCommand()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand<string>(x =>
            {
                Assert.IsNull(b);
                a = x;
            });
            var commandB = RxCommand.CreateFunction(() =>
            {
                Assert.AreEqual("foo", a);
                b = "done";
                return "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            var result = await combinedCommand.ExecuteAsync("foo");

            Assert.AreEqual("bar", result);
        }

        [Test]
        public async void CombineCommandWithFunctionInputSecondCommand()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateCommand(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxFunction<string>.CreateFunction(x =>
            {
                Assert.AreEqual("foo", a);
                b = "done";
                return x;
            });
            var combinedCommand = commandA.Combine(commandB);
            var result = await combinedCommand.ExecuteAsync("bar");

            Assert.AreEqual("bar", result);
        }

        [Test]
        public async void CombineFunctionWithFunctionPipelineNoInput()
        {
            string a = null;
            string b = null;
            var commandA = RxCommand.CreateFunction(() =>
            {
                Assert.IsNull(b);
                a = "done";
                return "foo";
            });
            var commandB = RxFunction<string>.CreateFunction(x =>
            {
                Assert.AreEqual("done", a);
                b = "done";
                return x + "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            var result = await combinedCommand.ExecuteAsync();

            Assert.AreEqual("foobar", result);
        }

        [Test]
        public async void CombineFunctionWithFunctionPipeline()
        {
            string a = null;
            string b = null;
            var commandA = RxFunction<string>.CreateFunction(x =>
            {
                Assert.IsNull(b);
                a = "done";
                return x;
            });
            var commandB = RxFunction<string>.CreateFunction(x =>
            {
                Assert.AreEqual("done", a);
                b = "done";
                return x + "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            var result = await combinedCommand.ExecuteAsync("foo");

            Assert.AreEqual("foobar", result);
        }
    }
}