namespace SexyReact.Views
{
    public class RxViewObject<T> : RxObjectMixin, IRxViewObjectMixin<T>
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

