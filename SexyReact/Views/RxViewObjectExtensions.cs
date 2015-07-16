using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Linq;
using System.Reactive.Disposables;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class RxViewObjectExtensions
    {
        public static IDisposable Connect<TViewTarget, TViewValue, TModel, TModelValue>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TViewTarget, TViewValue>> viewProperty,
            Expression<Func<TModel, TModelValue>> modelProperty,
            Func<TModelValue, TViewValue> converter = null
        )
            where TModel : IRxObject
        {
            converter = converter ?? (x => (TViewValue)Convert.ChangeType(x, typeof(TViewValue)));

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
                Stack<MemberExpression> stack = new Stack<MemberExpression>();
                MemberExpression member = setMainMember;
                while (member != null)
                {
                    stack.Push(member);
                    member = member.Expression as MemberExpression;
                }

                Expression target = Expression.Constant(viewTarget);
                while (stack.Any())
                {
                    var expression = stack.Pop();
                    target = Expression.MakeMemberAccess(target, expression.Member);
                }

                var value = Expression.Parameter(typeof(TViewValue));

                var body = Expression.Assign(target, value);
                var lambda = Expression.Lambda<Action<TViewValue>>(body, value);
                return lambda.Compile();
            };
            Lazy<Action<TViewValue>> setValue = new Lazy<Action<TViewValue>>(createSetValue);

            var result = view
                .ObserveProperty<IRxViewObject<TModel>, TModelValue>(propertyPath)
                .Subscribe(x => setValue.Value(converter(x)));

            return result;
        }

        public static IDisposable Biconnect<TView, TViewValue, TModel, TModelValue>(
            this TView view, 
            Expression<Func<TView, TViewValue>> viewProperty,
            Expression<Func<TModel, TModelValue>> modelProperty,
            Func<TModelValue, TViewValue> toViewValue = null,
            Func<TViewValue, TModelValue> toModelValue = null
        )
            where TView : IRxViewObject<TModel>
            where TModel : IRxObject
        {
            toModelValue = toModelValue ?? (x => (TModelValue)Convert.ChangeType(x, typeof(TModelValue)));

            var connectDisposable = (IDisposable)null;//view.Connect(viewProperty, modelProperty, toViewValue);

            var setMainMember = modelProperty.Body as MemberExpression;
            if (setMainMember == null)
                throw new ArgumentException("Lambda expression must specify a property path of the form (Foo.Bar.FooBar)", "modelProperty");

            Func<Action<TModelValue>> createSetValue = () =>
            {
                Stack<MemberExpression> stack = new Stack<MemberExpression>();
                MemberExpression member = setMainMember;
                while (member != null)
                {
                    stack.Push(member);
                    member = member.Expression as MemberExpression;
                }

                Expression target = Expression.Constant(view);
                while (stack.Any())
                {
                    var expression = stack.Pop();
                    target = Expression.MakeMemberAccess(target, expression.Member);
                }

                var value = Expression.Parameter(typeof(TModelValue));

                var body = Expression.Assign(target, value);
                var lambda = Expression.Lambda<Action<TModelValue>>(body, value);
                return lambda.Compile();
            };
            Lazy<Action<TModelValue>> setValue = new Lazy<Action<TModelValue>>(createSetValue);

            var result = view
                .ObserveProperty(viewProperty)
                .Subscribe(x => setValue.Value(toModelValue(x)));

            return new CompositeDisposable(connectDisposable, result);
        }
         
        private static class ReflectionCache<TModel>
            where TModel : IRxObject
        {
            public static readonly PropertyInfo viewObjectModelProperty = typeof(IRxViewObject<TModel>).GetProperty("Model");
        }
    }
}

