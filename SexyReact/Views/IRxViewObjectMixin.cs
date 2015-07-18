using System;

namespace SexyReact.Views
{
    public interface IRxViewObjectMixin<T> : IRxViewObject<T>, IRxObjectMixin
        where T : IRxObject
    {
    }
}

