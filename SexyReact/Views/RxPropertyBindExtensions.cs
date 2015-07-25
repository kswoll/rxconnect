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
            var setValue = binder.CreateViewPropertySetter(view, viewProperty);
            var result = binder.ViewObject
                .ObserveModelProperty(binder.ModelProperty)
                .SubscribeOnUiThread(x => setValue(converter(x)));
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
            var result = binder.ViewObject
                .ObserveModelProperty(binder.ModelProperty)
                .SubscribeOnUiThread(x => setValue(x));
            return result;
        }

        public static IDisposable To<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            Action<TModelValue> setter
        )
            where TModel : IRxObject
        {
            var result = binder.ViewObject
                .ObserveModelProperty(binder.ModelProperty)
                .SubscribeOnUiThread(x => setter(x));
            return result;
        }

        public static IDisposable Mate<TModel, TModelValue, TView, TViewValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            TView view, 
            Expression<Func<TView, TViewValue>> viewProperty,
            Func<TModelValue, TViewValue> toViewValue = null,
            Func<TViewValue, TModelValue> toModelValue = null
        )
            where TView : IRxObject
            where TModel : IRxObject
        {
            toViewValue = toViewValue ?? (x => (TViewValue)Convert.ChangeType(x, typeof(TViewValue)));
            toModelValue = toModelValue ?? (x => (TModelValue)Convert.ChangeType(x, typeof(TModelValue)));

            var connectDisposable = binder.To(view, viewProperty, toViewValue);
            Lazy<Action<TModelValue>> setValue = new Lazy<Action<TModelValue>>(() => binder.ViewObject.CreateModelPropertySetter(view, binder.ModelProperty));

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
            var connectDisposable = binder.To(view, viewProperty);
            Lazy<Action<TValue>> setValue = new Lazy<Action<TValue>>(() => binder.ViewObject.CreateModelPropertySetter(view, binder.ModelProperty));

            var result = view
                .ObserveProperty(viewProperty)
                .Subscribe(x => setValue.Value(x));

            return new CompositeDisposable(connectDisposable, result);
        }

        public static IObservable<TModelValue> ObserveModelProperty<TModel, TModelValue>(
            this IRxViewObject<TModel> view,
            Expression<Func<TModel, TModelValue>> modelProperty
        )
            where TModel : IRxObject
        {
            var remainingPath = modelProperty.GetPropertyPath();
            var propertyPath = new PropertyInfo[remainingPath.Length + 1];
            propertyPath[0] = ReflectionCache<TModel>.ViewObjectModelProperty;
            for (var i = 0; i < remainingPath.Length; i++)
            {
                propertyPath[i + 1] = remainingPath[i];
            }
            return new RxPropertyObservable<TModelValue>(view, propertyPath);
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
                throw new ArgumentException("Lambda expression must specify a property path of the form (Foo.Bar.FooBar)", "viewProperty");

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

                var memberType = expression.Member is FieldInfo ? ((FieldInfo)expression.Member).FieldType : ((PropertyInfo)expression.Member).PropertyType;
                if (!memberType.IsValueType && stack.Any())
                    predicate = Expression.OrElse(predicate, Expression.Equal(target, Expression.Constant(null)));
            }
            var value = Expression.Parameter(typeof(TViewValue));
            var body = Expression.IfThen(Expression.Not(predicate), Expression.Assign(target, value));
            var lambda = Expression.Lambda<Action<TViewValue>>(body, value);
            return lambda.Compile();
        }

        public static Action<TModelValue> CreateModelPropertySetter<TViewTarget, TModel, TModelValue>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TModel, TModelValue>> modelProperty
        )
            where TModel : IRxObject
        {
            var setMainMember = modelProperty.Body as MemberExpression;
            if (setMainMember == null)
                throw new ArgumentException("Lambda expression must specify a property path of the form (Foo.Bar.FooBar)", "modelProperty");

            Stack<MemberExpression> stack = new Stack<MemberExpression>();
            MemberExpression member = setMainMember;
            while (member != null)
            {
                stack.Push(member);
                member = member.Expression as MemberExpression;
            }

            Expression target = Expression.Constant(view);

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
