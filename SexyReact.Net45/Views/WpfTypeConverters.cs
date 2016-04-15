using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using SexyReact.Utils;
using SexyReact.Views.ValueConverters;

namespace SexyReact.Views
{
    public static class WpfTypeConverters
    {
        private static Dictionary<Tuple<Type, Type>, IValueConverter> converters = new Dictionary<Tuple<Type, Type>, IValueConverter>();

        static WpfTypeConverters()
        {
            converters[Tuple.Create(typeof(int), typeof(GridLength))] = new GridLengthAsIntConverter();
        }

        public static IValueConverter GetTypeConverter<TModel, TValue>(Expression<Func<TModel, TValue>> modelProperty, DependencyProperty viewProperty)
        {
            IValueConverter result;
            converters.TryGetValue(Tuple.Create(modelProperty.GetPropertyInfo().PropertyType, viewProperty.PropertyType), out result);
            return result;
        }
    }
}