using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;
using SexyReact.Utils;

namespace SexyReact.Views
{
    /// <summary>
    /// Binds model properties to view properties.
    /// </summary>
    public static class RxPropertyBindExtensions
    {
        public static IDisposable To<TModel, TModelValue, TView, TViewValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Expression<Func<TView, TViewValue>> viewProperty,
            Func<TModelValue, TViewValue> converter
        )
            where TModel : IRxObject
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            var setValue = binder.CreateViewPropertySetter(view, viewProperty);
            var result = binder
                .ObserveModelProperty()
                .SubscribeOnUiThread(x => 
                {
                    setValue(converter(x));
                });
            return result;
        }
                
        public static IDisposable To<TModel, TView, TValue>(
            this RxViewObjectBinder<TModel, TValue> binder,
            TView view, 
            Expression<Func<TView, TValue>> viewProperty
        )
            where TModel : IRxObject
        {
            var setValue = binder.CreateViewPropertySetter(view, viewProperty);
            var result = binder
                .ObserveModelProperty()
                .SubscribeOnUiThread(x => setValue(x));
            return result;
        }

        public static IDisposable Mate<TModel, TModelValue, TView, TViewValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Expression<Func<TView, TViewValue>> viewProperty,
            Func<TModelValue, TViewValue> toViewValue,
            Func<TViewValue, TModelValue> toModelValue
        )
            where TView : IRxObject
            where TModel : IRxObject
        {
            if (toViewValue == null)
                throw new ArgumentNullException(nameof(toViewValue));
            if (toModelValue == null)
                throw new ArgumentNullException(nameof(toModelValue));

            var connectDisposable = binder.To(view, viewProperty, toViewValue);
            var setValue = new Lazy<Action<TModelValue>>(() => binder.CreateModelPropertySetter());

            var result = view
                .ObserveProperty(viewProperty)
                .Subscribe(x => setValue.Value(toModelValue(x)));

            return new CompositeDisposable(connectDisposable, result);
        }

        public static IDisposable Mate<TModel, TView, TValue>(
            this RxViewObjectBinder<TModel, TValue> binder,
            TView view, 
            Expression<Func<TView, TValue>> viewProperty
        )
            where TView : IRxObject
            where TModel : IRxObject
        {
            return binder.Mate(view, viewProperty, x => x, x => x);
        }

        public static IObservable<TModelValue> ObserveModelProperty<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder
        )
            where TModel : IRxObject
        {
            var remainingPath = binder.ModelProperty.GetPropertyPath();
            var propertyPath = new PropertyInfo[remainingPath.Length + 1];
            propertyPath[0] = ReflectionCache<TModel>.ViewObjectModelProperty;
            for (var i = 0; i < remainingPath.Length; i++)
            {
                propertyPath[i + 1] = remainingPath[i];
            }
            return new RxPropertyObservable<TModelValue>(binder.ViewObject, propertyPath);
        }

        public static Action<TViewValue> CreateViewPropertySetter<TModel, TModelValue, TView, TViewValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Expression<Func<TView, TViewValue>> viewProperty
        )
            where TModel : IRxObject
        {
            var setMainMember = viewProperty.Body as MemberExpression;
            if (setMainMember == null)
                throw new ArgumentException("Lambda expression must specify a property path of the form (Foo.Bar.FooBar)", nameof(viewProperty));

            Stack<MemberExpression> stack = new Stack<MemberExpression>();
            MemberExpression member = setMainMember;
            while (member != null)
            {
                stack.Push(member);
                member = member.Expression as MemberExpression;
            }

            Expression target = Expression.Constant(view);
            Expression predicate = Expression.Equal(Expression.Constant(view), Expression.Constant(null));
            while (stack.Any())
            {
                var expression = stack.Pop();
                target = Expression.MakeMemberAccess(target, expression.Member);

                var memberType = (expression.Member as FieldInfo)?.FieldType ?? ((PropertyInfo)expression.Member).PropertyType;
                if (!memberType.IsValueType && stack.Any())
                    predicate = Expression.OrElse(predicate, Expression.Equal(target, Expression.Constant(null)));
            }
            var value = Expression.Parameter(typeof(TViewValue));
            var body = Expression.IfThen(Expression.Not(predicate), Expression.Assign(target, value));
            var lambda = Expression.Lambda<Action<TViewValue>>(body, value);
            return lambda.Compile();
        }

        public static Action<TModelValue> CreateModelPropertySetter<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder
        )
            where TModel : IRxObject
        {
            var setMainMember = binder.ModelProperty.Body as MemberExpression;
            if (setMainMember == null)
                throw new ArgumentException("Lambda expression must specify a property path of the form (Foo.Bar.FooBar)", "binder");

            Stack<MemberExpression> stack = new Stack<MemberExpression>();
            MemberExpression member = setMainMember;
            while (member != null)
            {
                stack.Push(member);
                member = member.Expression as MemberExpression;
            }

            Expression target = Expression.Constant(binder.ViewObject);

            // view.Model
            target = Expression.MakeMemberAccess(target, ReflectionCache<TModel>.ViewObjectModelProperty);

            while (stack.Any())
            {
                var expression = stack.Pop();
                target = Expression.MakeMemberAccess(target, expression.Member);
            }

            var value = Expression.Parameter(typeof(TModelValue));

            var body = Expression.Assign(target, value);
            var lambda = Expression.Lambda<Action<TModelValue>>(body, value);
            return lambda.Compile();
        }

        private static class ReflectionCache<TModel>
            where TModel : IRxObject
        {
            public static readonly PropertyInfo ViewObjectModelProperty = typeof(IRxViewObject<TModel>).GetProperty("Model");
        }
    }
}
