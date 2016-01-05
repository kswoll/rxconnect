using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SexyReact.Utils;

namespace SexyReact.Views
{
    public static class RxInputBindExtensions
    {
        public static IDisposable Mate<TModel>(
            this RxViewObjectBinder<TModel, string> binder,
            TextBox view
        )
            where TModel : IRxObject
        {
            return binder.Mate(view, x => x.Text, x => new TextChangedEventHandler((sender, args) => x()), (x, l) => x.TextChanged += l, (x, l) => x.TextChanged -= l);
        }

        public static void To<TModel>(
            this RxViewObjectBinder<TModel, string> binder,
            FrameworkElementFactory factory,
            DependencyProperty source
        )
            where TModel : IRxObject
        {
            factory.SetValue(source, new Binding(binder.ModelProperty.GetPropertyInfo().Name));
        }
    }
}