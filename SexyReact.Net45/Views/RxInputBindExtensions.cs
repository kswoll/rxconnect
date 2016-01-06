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

        public static IDisposable Mate<TModel, TValue>(
            this RxViewObjectBinder<TModel, TValue> binder,
            ComboBox view
        )
            where TModel : IRxObject
        {
            return binder.Mate(
                view, 
                x => x.SelectedValue, 
                x => new SelectionChangedEventHandler((sender, args) =>
                {
                    if (args.AddedItems.Count > 0)
                    {
                        x();
                    }
                }), 
                (x, l) => x.SelectionChanged += l, 
                (x, l) => x.SelectionChanged -= l, 
                x => x, 
                x => x == null ? default(TValue) : (TValue)x);
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