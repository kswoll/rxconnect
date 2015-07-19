using System;
using System.Reactive.Linq;
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
            var command = RxCommand.Create(() => s = "foo");
            await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void CommandNoInputExecute()
        {
            string s = null;
            var command = RxCommand.Create(() => s = "foo");
            command.Execute();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public async void CommandWithInputExecuteAsync()
        {
            string s = null;
            var command = RxCommand.Create<string>(x => s = x);
            await command.ExecuteAsync("foo");
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void CommandWithInputExecute()
        {
            string s = null;
            var command = RxCommand.Create<string>(x => s = x);
            command.Execute("foo");
            Assert.AreEqual("foo", s);
        }

        [Test]
        public async void FunctionNoInputExecuteAsync()
        {
            string observed = null;
            var command = RxFunction.Create(() => "foo");
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public void FunctionNoInputExecute()
        {
            string observed = null;
            var command = RxFunction.Create(() => "foo");
            command.Subscribe(x => observed = x);
            var s = command.Execute();
            Assert.AreEqual("foo", s);            
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public async void FunctionWithInputExecuteAsync()
        {
            string observed = null;
            var command = RxFunction<string>.Create(x => x);
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync("foo");
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public void FunctionWithInputExecute()
        {
            string observed = null;
            var command = RxFunction<string>.Create(x => x);
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
            var command = RxCommand.Create(async () => s = await GetFoo());
            await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public void AsyncCommandNoInputExecute()
        {
            string s = null;
            var command = RxCommand.Create(async () => s = await GetFoo());
            command.Execute();
            Assert.AreEqual("foo", s);
        }

        [Test]
        public async void AsyncCommandWithInputExecuteAsync()
        {
            string s = null;
            var command = RxCommand.Create<string>(async x => s = x + await GetFoo());
            await command.ExecuteAsync("foo");
            Assert.AreEqual("foofoo", s);
        }

        [Test]
        public void AsyncCommandWithInputExecute()
        {
            string s = null;
            var command = RxCommand.Create<string>(async x => s = x + await GetFoo());
            command.Execute("foo");
            Assert.AreEqual("foofoo", s);
        }

        [Test]
        public async void AsyncFunctionNoInputExecuteAsync()
        {
            string observed = null;
            var command = RxFunction.CreateAsync(async () => await GetFoo());
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync();
            Assert.AreEqual("foo", s);
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public void AsyncFunctionNoInputExecute()
        {
            string observed = null;
            var command = RxFunction.CreateAsync(async () => await GetFoo());
            command.Subscribe(x => observed = x);
            var s = command.Execute();
            Assert.AreEqual("foo", s);            
            Assert.AreEqual("foo", observed);
        }

        [Test]
        public async void AsyncFunctionWithInputExecuteAsync()
        {
            string observed = null;
            var command = RxFunction<string>.CreateAsync(async x => x + await GetFoo());
            command.Subscribe(x => observed = x);
            var s = await command.ExecuteAsync("foo");
            Assert.AreEqual("foofoo", s);
            Assert.AreEqual("foofoo", observed);
        }

        [Test]
        public void AsyncFunctionWithInputExecute()
        {
            string observed = null;
            var command = RxFunction<string>.CreateAsync(async x => x + await GetFoo());
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
            var commandA = RxCommand.Create(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxCommand.Create(() =>
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
            var commandA = RxCommand.Create(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxCommand.Create<string>(x =>
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
            var commandA = RxCommand.Create<string>(x =>
            {
                Assert.IsNull(b);
                a = x;
            });
            var commandB = RxCommand.Create(() =>
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
            var commandA = RxCommand.Create<string>(x =>
            {
                Assert.IsNull(b);
                a = x;
            });
            var commandB = RxCommand.Create<string>(x =>
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
            var commandA = RxCommand.Create(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxFunction.Create(() =>
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
            var commandA = RxCommand.Create<string>(x =>
            {
                Assert.IsNull(b);
                a = x;
            });
            var commandB = RxFunction.Create(() =>
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
            var commandA = RxCommand.Create(() =>
            {
                Assert.IsNull(b);
                a = "foo";
            });
            var commandB = RxFunction<string>.Create(x =>
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
            var commandA = RxFunction.Create(() =>
            {
                Assert.IsNull(b);
                a = "done";
                return "foo";
            });
            var commandB = RxFunction<string>.Create(x =>
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
            var commandA = RxFunction<string>.Create(x =>
            {
                Assert.IsNull(b);
                a = "done";
                return x;
            });
            var commandB = RxFunction<string>.Create(x =>
            {
                Assert.AreEqual("done", a);
                b = "done";
                return x + "bar";
            });
            var combinedCommand = commandA.Combine(commandB);
            var result = await combinedCommand.ExecuteAsync("foo");

            Assert.AreEqual("foobar", result);
        }

        [Test]
        public async void CannotRunConcurrently()
        {
            IRxFunction<string> function = null;
            function = RxFunction.CreateAsync(async () =>
            {
                var secondInvocation = await function.ExecuteAsync();
                Assert.IsNull(secondInvocation);
                return "foo";
            });

            var result = await function.ExecuteAsync();
            Assert.AreEqual("foo", result);
        }

        [Test]
        public async void CanRunConcurrently()
        {
            int stackDepth = 0;
            IRxFunction<string> function = null;
            function = RxFunction.CreateAsync(async () =>
            {
                if (stackDepth == 1)
                    return "complete";
                stackDepth++;
                var secondInvocation = await function.ExecuteAsync();
                return secondInvocation;
            }, allowSimultaneousExecution: true);

            var result = await function.ExecuteAsync();
            Assert.AreEqual("complete", result);
        }

        [Test]
        public async void CanExecuteFalse()
        {
            var command = RxFunction.Create(() => GetFoo(), Observable.Return(false));
            var result = await command.ExecuteAsync();
            Assert.IsNull(result);
        }

        [Test]
        public async void CanExecuteTrueCannotExecuteConcurrently()
        {
            int stackDepth = 0;
            IRxFunction<string> function = null;
            function = RxFunction.CreateAsync(async () =>
            {
                if (stackDepth == 1)
                    Assert.Fail("Execution is recursing");
                stackDepth++;

                var secondInvocation = await function.ExecuteAsync();
                Assert.IsNull(secondInvocation);
                return "foo";
            }, Observable.Return(true));

            var result = await function.ExecuteAsync();
            Assert.AreEqual("foo", result);
        }

        [Test]
        public async void CanExecuteEmitsNoValue()
        {
            var canExecute = new Subject<bool>();
            var command = RxFunction.CreateAsync(() => GetFoo(), canExecute);
            var result = await command.ExecuteAsync();
            Assert.AreEqual("foo", result);
        }

        [Test]
        public async void ToggleCanExecute()
        {
            var canExecute = new Subject<bool>();
            var command = RxFunction.CreateAsync(() => GetFoo(), canExecute);
            var result = await command.ExecuteAsync();

            Assert.AreEqual("foo", result);
            canExecute.OnNext(false);
            result = await command.ExecuteAsync();
            Assert.IsNull(result);
            canExecute.OnNext(true);
            result = await command.ExecuteAsync();
            Assert.AreEqual("foo", result);
        }

        [Test]
        public async void ExecuteTwice()
        {
            var command = RxFunction.CreateAsync(() => GetFoo());
            var result = await command.ExecuteAsync();
            Assert.AreEqual("foo", result);
            result = await command.ExecuteAsync();
            Assert.AreEqual("foo", result);
        }
    }
}