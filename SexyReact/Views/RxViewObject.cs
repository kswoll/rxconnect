using System;
using System.Linq.Expressions;

namespace SexyReact.Views
{
    public class RxViewObject<T> : RxObjectMixin, IRxViewObject<T>, IRxViewObjectMixin<T>
        where T : IRxObject
    {
        public T Model { get { return Get<T>(); } set { Set(value); } }

        object IRxViewObject.Model
        {
            get { return Model; }
            set { Model = (T)value; }
        }

        protected override void Dispose(bool isDisposing)
        {
        }
    }
}

