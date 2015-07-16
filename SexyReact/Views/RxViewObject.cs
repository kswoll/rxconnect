using System;
using System.Linq.Expressions;

namespace SexyReact.Views
{
    public class RxViewObject<T> : RxObject, IRxViewObject<T>
        where T : IRxObject
    {
        public T Model { get { return Get<T>(); } set { Set(value); } }

        protected override void Dispose(bool isDisposing)
        {
        }
    }
}

