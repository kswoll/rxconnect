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

        public static IEnumerable<MemberInfo> GetPropertyPath(this LambdaExpression expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
            {
                return Enumerable.Empty<MemberInfo>();
            }
            return member
                .SelectRecursive(x => x.Expression as MemberExpression)
                .Select(x => x.Member).Reverse();
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
    }
}