using System;

namespace SexyReact.Views
{
    public interface IRxViewObject<T> : IRxObject
        where T : IRxObject
    {
        T Model { get; set; }
    }
}

