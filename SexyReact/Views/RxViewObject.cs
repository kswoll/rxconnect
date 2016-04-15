using System.Reflection;

namespace SexyReact.Views
{
    public class RxViewObject<T> : RxObjectMixin, IRxViewObjectMixin<T>
        where T : IRxObject
    {
        public static readonly PropertyInfo ViewObjectModelProperty = typeof(IRxViewObject<T>).GetProperty("Model");

        public T Model { get { return Get<T>(); } set { Set<T>(ViewObjectModelProperty, value); } }

        public RxViewObject(IRxObject container) : base(new DictionaryStorageStrategy(), new DictionaryObservePropertyStrategy(container))
        {
        }

        object IRxViewObject.Model
        {
            get { return Model; }
            set { Model = (T)value; }
        }

//        protected override void Dispose(bool isDisposing)
//        {
//        }
    }
}

