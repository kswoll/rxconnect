using System;

namespace RxConnect.Views
{
    public class RxViewObject<T> : RxObject, IRxViewObject<T>
        where T : IRxObject
    {
        public T Model { get { return Get<T>(); } set { Set<T>(value); } }

        public RxViewObject()
        {
        }

        public void Dispose()
        {
        }
    }
}

