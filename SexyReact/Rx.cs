using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;

namespace SexyReact
{
    public static class Rx
    {
        public static IScheduler UiScheduler { get; set; }
    }
}
