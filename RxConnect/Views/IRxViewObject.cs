using System;

namespace RxConnect.Views
{
    public interface IRxViewObject<T> : IRxObject
        where T : IRxObject
    {
        T Model { get; set; }
    }
}

