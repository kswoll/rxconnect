using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Linq;
using RxConnect.Utils;

namespace RxConnect.Views
{
    public static class RxViewObjectExtensions
    {
        public static IDisposable Connect<TView, TModel, TModelValue, TViewValue>(
            this TView view, 
            Expression<Func<TModel, TModelValue>> modelProperty, 
            Expression<Func<TView, TViewValue>> viewProperty,
            Func<TModelValue, TViewValue> converter = null
        )
            where TView : IRxViewObject<TModel>
            where TModel : IRxObject
        {
            converter = converter ?? new Func<TModelValue, TViewValue>(x => (TViewValue)Convert.ChangeType(x, typeof(TViewValue)));

            var remainingPath = modelProperty.GetPropertyPath().ToArray();
            var propertyPath = new PropertyInfo[remainingPath.Length + 1];
            propertyPath[0] = ReflectionCache<TModel>.viewObjectModelProperty;
            for (var i = 0; i < remainingPath.Length; i++)
            {
                propertyPath[i + 1] = (PropertyInfo)remainingPath[i];
            }

            Func<Action<TViewValue>> createSetValue = () =>
            {
                return null;
            };
            Lazy<Action<TViewValue>> setValue = new Lazy<Action<TViewValue>>(createSetValue);

            var result = view
                .ObserveProperty<TView, TModelValue>(propertyPath)
                .Subscribe(x => setValue.Value(converter(x)));

            return result;
        }

        private static class ReflectionCache<TModel>
            where TModel : IRxObject
        {
            public static readonly PropertyInfo viewObjectModelProperty = typeof(IRxViewObject<TModel>).GetProperty("Model");
        }
    }
}

