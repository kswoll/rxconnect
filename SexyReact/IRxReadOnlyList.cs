using System;
using System.Collections.Generic;
using System.Reactive;

namespace SexyReact
{
    public interface IRxReadOnlyList<T> : IRxListObservables<T>, IReadOnlyList<T>, IDisposable
    {
    }
}
