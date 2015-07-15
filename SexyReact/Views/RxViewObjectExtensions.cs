using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Linq;
using SexyReact.Utils;

namespace SexyReact.Views
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

            var setMainMember = viewProperty.Body as MemberExpression;
            if (setMainMember == null)
                throw new ArgumentException("Lambda expression must specify a property path of the form (Foo.Bar.FooBar)", "viewProperty");

            Func<Action<TViewValue>> createSetValue = () =>
            {
                var currentMember = setMainMember;
                var currentTarget = currentMember.Expression;
                Expression body = null;
                do
                {
                    var currentTargetMemberAccess = currentTarget as MemberExpression;
                    if (currentTargetMemberAccess == null)
                    {
                        var rootTarget = currentTarget as ParameterExpression;
                        if (rootTarget == null)
                            throw new ArgumentException("Root of lambda expression (" + viewProperty + ") must be a reference to the parameter passed into the lambda.");

                        body = Expression.MakeMemberAccess(Expression.Constant(view), currentMember.Member);
                        break;
                    }
                    else 
                    {
                        currentMember = currentTargetMemberAccess;
                    }
                }
                while (true);

                var lambda = Expression.Lambda<Action<TViewValue>>(body);
                return lambda.Compile();
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

