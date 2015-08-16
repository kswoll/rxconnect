using System;
using System.Collections.Generic;
using System.Text;

namespace SexyReact
{
    public struct RxListObservedElement<TElement, TValue>
    {
        public TElement Element { get; }
        public TValue Value { get; }

        public RxListObservedElement(TElement element, TValue value)
        {
            Element = element;
            Value = value;
        }
    }
}
