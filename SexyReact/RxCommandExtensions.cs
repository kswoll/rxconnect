using System;

namespace SexyReact
{
    public static class RxCommandExtensions
    {
        public static void Execute(this IRxCommand command)
        {
            command.ExecuteAsync().Wait();
        }

        public static void Execute<TInput>(this IRxCommand<TInput> command, TInput input)
        {
            command.ExecuteAsync(input).Wait();
        }

        public static TOutput Execute<TOutput>(this IRxFunction<TOutput> command)
        {
            return command.ExecuteAsync().Result;
        }

        public static TOutput Execute<TInput, TOutput>(this IRxFunction<TInput, TOutput> command, TInput input)
        {
            return command.ExecuteAsync(input).Result;
        }
    }
}

