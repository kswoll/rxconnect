using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SexyReact.Utils
{
    internal static class TypeExtensions
    {
        private static IEnumerable<T> SelectRecursive<T>(this T obj, Func<T, T> next) where T : class
        {
            T current = obj;
            while (current != null)
            {
                yield return current;
                current = next(current);
            }
        }

        public static PropertyInfo[] GetPropertyPath(this LambdaExpression expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
            {
                return new PropertyInfo[0];
            }
            var firstProperty = member.Member as PropertyInfo;
            if (firstProperty == null)
            {
                throw new Exception(member.Member + " must be a property.");
            }
            var previousMember = member.Expression as MemberExpression;
            if (previousMember == null)
            {
                return new[] { firstProperty };
            }

            var previousProperty = previousMember.Member as PropertyInfo;
            if (previousProperty == null)
            {
                throw new Exception(previousMember.Member + " must be a property.");
            }
            var secondPreviousMember = previousMember.Expression as MemberExpression;
            if (secondPreviousMember == null)
            {
                return new[] { previousProperty, firstProperty };
            }

            var secondPreviousProperty = secondPreviousMember.Member as PropertyInfo;
            if (secondPreviousProperty == null)
            {
                throw new Exception(secondPreviousMember.Member + " must be a property.");                
            }
            var thirdPreviousMember = secondPreviousMember.Expression as MemberExpression;
            if (thirdPreviousMember == null)
            {
                return new[] { secondPreviousProperty, previousProperty, firstProperty };
            }
            
            Stack<PropertyInfo> stack = new Stack<PropertyInfo>();
            stack.Push(firstProperty);
            stack.Push(previousProperty);
            stack.Push(secondPreviousProperty);
            
            var current = thirdPreviousMember;
            while (current != null)
            {
                var property = current.Member as PropertyInfo;
                if (property == null)
                {
                    throw new Exception(current.Member + " must be a property.");                                    
                }
                stack.Push(property);
                current = current.Expression as MemberExpression;
            }

            return stack.ToArray();
        }
         
        public static MemberInfo GetMemberInfo(this LambdaExpression expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
                return null;
            return member.Member;
        }

        public static PropertyInfo GetPropertyInfo(this LambdaExpression expression)
        {
            return (PropertyInfo)expression.GetMemberInfo();
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            var property = member as PropertyInfo;
            if (property != null)
                return property.PropertyType;

            var field = member as FieldInfo;
            if (field != null)
                return field.FieldType;

            var method = member as MethodInfo;
            if (method != null)
                return method.ReturnType;

            var @event = member as EventInfo;
            if (@event != null)
                return @event.EventHandlerType;

            throw new ArgumentException("Member must be a property, field, method, or event", "member");

        }
    }
}