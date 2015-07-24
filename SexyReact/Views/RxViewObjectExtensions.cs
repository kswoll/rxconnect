using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using System.Reactive.Disposables;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class RxViewObjectExtensions
    {
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

        public static Action<TViewValue> CreateViewPropertySetter<TViewTarget, TViewValue, TModel>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TViewTarget, TViewValue>> viewProperty
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

            Expression target = Expression.Constant(viewTarget);
            Expression predicate = Expression.Equal(Expression.Constant(viewTarget), Expression.Constant(null));
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

        public static IDisposable Connect<TViewTarget, TViewValue, TModel, TModelValue>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TViewTarget, TViewValue>> viewProperty,
            Expression<Func<TModel, TModelValue>> modelProperty,
            Func<TModelValue, TViewValue> converter
        )
            where TModel : IRxObject
        {
            Action<TViewValue> setValue = CreateViewPropertySetter(view, viewTarget, viewProperty);
            var result = view
                .ObserveModelProperty(modelProperty)
                .SubscribeOnUiThread(x => setValue(converter(x)));

            return result;
        }
                
        public static IDisposable Connect<TViewTarget, TModel, TValue>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TViewTarget, TValue>> viewProperty,
            Expression<Func<TModel, TValue>> modelProperty
        )
            where TModel : IRxObject
        {
            Action<TValue> setValue = CreateViewPropertySetter(view, viewTarget, viewProperty);
            var result = view
                .ObserveModelProperty(modelProperty)
                .SubscribeOnUiThread(x => setValue(x));

            return result;
        }

        public static IDisposable Connect<TViewValue, TModel, TModelValue>(
            this IRxViewObject<TModel> view, 
            Action<TViewValue> viewSetter,
            Expression<Func<TModel, TModelValue>> modelProperty,
            Func<TModelValue, TViewValue> converter
        )
            where TModel : IRxObject
        {
            var result = view
                .ObserveModelProperty(modelProperty)
                .SubscribeOnUiThread(x => viewSetter(converter(x)));

            return result;
        }

        public static IDisposable Connect<TModel, TValue>(
            this IRxViewObject<TModel> view, 
            Action<TValue> viewSetter,
            Expression<Func<TModel, TValue>> modelProperty
        )
            where TModel : IRxObject
        {
            var result = view
                .ObserveModelProperty(modelProperty)
                .SubscribeOnUiThread(x => viewSetter(x));

            return result;
        }

        public static Action<TModelValue> CreateModelPropertySetter<TViewTarget, TModel, TModelValue>(
            IRxViewObject<TModel> view, 
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

        public static IDisposable Biconnect<TViewTarget, TViewValue, TModel, TModelValue>(
            this IRxViewObject<TModel> view, 
            TViewTarget viewTarget, 
            Expression<Func<TViewTarget, TViewValue>> viewProperty,
            Expression<Func<TModel, TModelValue>> modelProperty,
            Func<TModelValue, TViewValue> toViewValue = null,
            Func<TViewValue, TModelValue> toModelValue = null
        )
            where TViewTarget : IRxObject
            where TModel : IRxObject
        {
            toViewValue = toViewValue ?? (x => (TViewValue)Convert.ChangeType(x, typeof(TViewValue)));
            toModelValue = toModelValue ?? (x => (TModelValue)Convert.ChangeType(x, typeof(TModelValue)));

            var connectDisposable = view.Connect(viewTarget, viewProperty, modelProperty, toViewValue);
            Lazy<Action<TModelValue>> setValue = new Lazy<Action<TModelValue>>(() => CreateModelPropertySetter(view, viewTarget, modelProperty));

            var result = viewTarget
                .ObserveProperty(viewProperty)
                .Subscribe(x => setValue.Value(toModelValue(x)));

            return new CompositeDisposable(connectDisposable, result);
        }
         
        private static class ReflectionCache<TModel>
            where TModel : IRxObject
        {
            public static readonly PropertyInfo ViewObjectModelProperty = typeof(IRxViewObject<TModel>).GetProperty("Model");
        }
    }
}

