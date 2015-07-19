using System.Reactive;

namespace SexyReact
{
    public static class RxCommandExtensions
    {
        /// <summary>
        /// Executes the command synchronously by calling Wait() on the async task.  This should generally only be 
        /// called when you know the action will execute synchronously.  Otherwise you will likely face potential 
        /// deadlocks.
        /// </summary>
        public static void Execute(this IRxCommand command)
        {
            command.ExecuteAsync().Wait();
        }

        /// <summary>
        /// Executes the command synchronously by calling Wait() on the async task.  This should generally only be 
        /// called when you know the action will execute synchronously.  Otherwise you will likely face potential 
        /// deadlocks.
        /// </summary>
        public static void Execute<TInput>(this IRxCommand<TInput> command, TInput input)
        {
            command.ExecuteAsync(input).Wait();
        }

        /// <summary>
        /// Executes the command synchronously by returning Result on the async task.  This should generally only be 
        /// called when you know the action will execute synchronously.  Otherwise you will likely face potential 
        /// deadlocks.
        /// </summary>
        public static TOutput Execute<TOutput>(this IRxFunction<TOutput> command)
        {
            return command.ExecuteAsync().Result;
        }

        /// <summary>
        /// Executes the command synchronously by directly returning Result on the async task.  This should generally only be 
        /// called when you know the action will execute synchronously.  Otherwise you will likely face potential 
        /// deadlocks.
        /// </summary>
        public static TOutput Execute<TInput, TOutput>(this IRxFunction<TInput, TOutput> command, TInput input)
        {
            return command.ExecuteAsync(input).Result;
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxCommand Combine(this IRxCommand first, IRxCommand second)
        {
            return RxCommand.Create(async () =>
            {
                await first.ExecuteAsync();
                await second.ExecuteAsync();
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The first command 
        /// expects TInput as an argument, and the returned command also expects TInput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxCommand<TInput> Combine<TInput>(this IRxCommand<TInput> first, IRxCommand second)
        {
            return RxCommand.Create<TInput>(async x =>
            {
                await first.ExecuteAsync(x);
                await second.ExecuteAsync();
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  Both commands
        /// expect TInput as an argument, and the returned command also expects TInput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxCommand<TInput> Combine<TInput>(this IRxCommand<TInput> first, IRxCommand<TInput> second)
        {
            return RxCommand.Create<TInput>(async x =>
            {
                await first.ExecuteAsync(x);
                await second.ExecuteAsync(x);
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The second command
        /// expects TInput as an argument, and the returned command also expects TInput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxCommand<TInput> Combine<TInput>(this IRxCommand first, IRxCommand<TInput> second)
        {
            return RxCommand.Create<TInput>(async x =>
            {
                await first.ExecuteAsync();
                await second.ExecuteAsync(x);
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The second command
        /// returns TOutput, and the returned command also returns TOutput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TOutput> Combine<TOutput>(this IRxCommand first, IRxFunction<TOutput> second)
        {
            return RxFunction.CreateAsync(async () =>
            {
                await first.ExecuteAsync();
                return await second.ExecuteAsync();
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The second command
        /// expects TInput as a parameter and returns TOutput.  The returned command expects TInput and returns 
        /// TOutput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TOutput> Combine<TInput, TOutput>(this IRxCommand first, IRxFunction<TInput, TOutput> second)
        {
            return RxFunction.CreateAsync<TInput, TOutput>(async x =>
            {
                await first.ExecuteAsync();
                return await second.ExecuteAsync(x);
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The first command
        /// expects TInput as a parameter.  The second command returns TOutput. The returned command expects 
        /// TInput and returns TOutput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TOutput> Combine<TInput, TOutput>(this IRxCommand<TInput> first, IRxFunction<TOutput> second)
        {
            return RxFunction.CreateAsync<TInput, TOutput>(async x =>
            {
                await first.ExecuteAsync(x);
                return await second.ExecuteAsync();
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  Both commands
        /// expect TInput as a parameter.  The second command returns TOutput. The returned command expects 
        /// TInput and returns TOutput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TOutput> Combine<TInput, TOutput>(this IRxCommand<TInput> first, IRxFunction<TInput, TOutput> second)
        {
            return RxFunction.CreateAsync<TInput, TOutput>(async x =>
            {
                await first.ExecuteAsync(x);
                return await second.ExecuteAsync(x);
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The first command
        /// returns TOutput.  The returned command also returns TOutput.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TOutput> Combine<TOutput>(this IRxFunction<TOutput> first, IRxCommand second)
        {
            return RxFunction.CreateAsync(async () =>
            {
                var result = await first.ExecuteAsync();
                await second.ExecuteAsync();
                return result;
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The first command 
        /// expects TInput as an argument and returns TOutput. The returned command also expects TInput when executed
        /// and returns TOutput.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TOutput> Combine<TInput, TOutput>(this IRxFunction<TInput, TOutput> first, IRxCommand second)
        {
            return RxFunction.CreateAsync<TInput, TOutput>(async x =>
            {
                var result = await first.ExecuteAsync(x);
                await second.ExecuteAsync();
                return result;
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  Both commands
        /// expect TInput as an argument, and the second command also returns TOutput.  The returned command 
        /// also expects TInput when executed and returns TOutput.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TOutput> Combine<TInput, TOutput>(this IRxFunction<TInput, TOutput> first, IRxCommand<TInput> second)
        {
            return RxFunction.CreateAsync<TInput, TOutput>(async x =>
            {
                var result = await first.ExecuteAsync(x);
                await second.ExecuteAsync(x);
                return result;
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The first command returns
        /// TOutput. The second command expects TInput as an argument. The returned command also expects TInput when executed 
        /// and returns TOutput.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TOutput> Combine<TInput, TOutput>(this IRxFunction<TOutput> first, IRxCommand<TInput> second)
        {
            return RxFunction.CreateAsync<TInput, TOutput>(async x =>
            {
                var result = await first.ExecuteAsync();
                await second.ExecuteAsync(x);
                return result;
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  The output of the first command
        /// is fed to the input of the second command.  The returned command returns TSecondOutput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TSecondOutput> Combine<TFirstOutput, TSecondOutput>(this IRxFunction<TFirstOutput> first, IRxFunction<TFirstOutput, TSecondOutput> second)
        {
            return RxFunction.CreateAsync(async () =>
            {
                var firstResult = await first.ExecuteAsync();
                return await second.ExecuteAsync(firstResult);
            });
        }

        /// <summary>
        /// Executes the first command and then when it completes executes the second command.  TInput is passed to the 
        /// first command and the output of that command is fed in as the argument to the seond command. The returned command 
        /// expects TInput and returns TSecondOutput when executed.
        /// </summary>
        /// <param name="first">The command to execute first</param>
        /// <param name="second">The command to execute second</param>
        /// <returns>The new command that executes both commands in turn.</returns>
        public static IRxFunction<TInput, TSecondOutput> Combine<TInput, TFirstOutput, TSecondOutput>(this IRxFunction<TInput, TFirstOutput> first, IRxFunction<TFirstOutput, TSecondOutput> second)
        {
            return RxFunction.CreateAsync<TInput, TSecondOutput>(async x =>
            {
                var firstResult = await first.ExecuteAsync(x);
                return await second.ExecuteAsync(firstResult);
            });
        }

        /// <summary>
        /// Converts an unparameterized command into a command parameterized by Unit.  This can be useful with methods such 
        /// as Combine which expect particular types of commands that may not fit the command you have in order to
        /// get the behavor you otherwise expect.  
        /// </summary>
        public static IRxCommand<Unit> AsParameterized(this IRxCommand command)
        {
            return RxCommand.Create<Unit>(_ => command.ExecuteAsync());
        }

        /// <summary>
        /// Converts an unparameterized command into a command parameterized by Unit.  This can be useful with methods such 
        /// as Combine which expect particular types of commands that may not fit the command you have in order to
        /// get the behavor you otherwise expect.  
        /// </summary>
        public static IRxFunction<Unit, TOutput> AsParameterized<TOutput>(this IRxFunction<TOutput> command)
        {
            return RxFunction.CreateAsync<Unit, TOutput>(_ => command.ExecuteAsync());
        }

        /// <summary>
        /// Converts an IRxFunction&lt;TOutput&gt; into an IRxCommand.  This can be useful with methods such as 
        /// Combine which expect particular types of commands that may not fit the command you have in order to
        /// get the behavor you otherwise expect.  The value returned from the function is simply discarded.
        /// </summary>
        public static IRxCommand AsCommand<TOutput>(this IRxFunction<TOutput> function)
        {
            return RxCommand.Create(() => function.ExecuteAsync());
        }

        /// <summary>
        /// Converts an IRxFunction&lt;TOutput&gt; into an IRxCommand.  This can be useful with methods such as 
        /// Combine which expect particular types of commands that may not fit the command you have in order to
        /// get the behavor you otherwise expect.  The value returned from the function is simply discarded.
        /// </summary>
        public static IRxCommand<TInput> AsCommand<TInput, TOutput>(this IRxFunction<TInput, TOutput> function)
        {
            return RxCommand.Create<TInput>(x => function.ExecuteAsync(x));
        }
    }
}