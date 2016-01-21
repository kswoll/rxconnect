using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            return binder.Mate(view, x => x.Text, x => new TextChangedEventHandler((sender, args) => x(view.Text)), (x, l) => x.TextChanged += l, (x, l) => x.TextChanged -= l);
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
                        x(args.AddedItems[0]);
                    }
                }), 
                (x, l) => x.SelectionChanged += l, 
                (x, l) => x.SelectionChanged -= l, 
                x => x, 
                x => x == null ? default(TValue) : (TValue)x);
        }

        public static IDisposable Mate<TModel, TValue>(
            this RxViewObjectBinder<TModel, TValue> binder,
            ComboBox view,
            Expression<Func<TModel, IEnumerable<TValue>>> itemsSource
        )
            where TModel : IRxObject
        {
            bool settingItemsSource = false;
            binder.ViewObject.Bind(itemsSource).To(x =>
            {
                settingItemsSource = true;
                view.ItemsSource = x;
                settingItemsSource = false;
            });

            return binder.Mate(
                view, 
                x => x.SelectedValue, 
                x => new SelectionChangedEventHandler((sender, args) =>
                {
                    if (args.AddedItems.Count > 0 && !settingItemsSource)
                    {
                        x(args.AddedItems[0]);
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

        public static void To<TModel, TValue>(
            this RxViewObjectBinder<TModel, IEnumerable<TValue>> binder,
            ListView listView
        )
            where TModel : IRxObject
        {
            binder.To(x => listView.ItemsSource = x);
        }

        public static void To<TModel, TItem>(
            this RxViewObjectBinder<TModel, RxList<TItem>> binder,
            ListView listView,
            Expression<Func<TModel, TItem>> selectedValueModelProperty
        )
            where TModel : IRxObject
        {
            binder.To(x => listView.ItemsSource = x);
            binder.ViewObject.Bind(selectedValueModelProperty).Mate(
                listView, 
                x => x.SelectedValue, 
                x => new SelectionChangedEventHandler((sender, args) =>
                {
                    if (args.AddedItems.Count > 0)
                    {
                        x(args.AddedItems[0]);
                    }
                }), 
                (view, l) => view.SelectionChanged += l, 
                (view, l) => view.SelectionChanged -= l, 
                x => x, 
                x => x == null ? default(TItem) : (TItem)x);
        }

        public static DataGridTextColumn AddTextColumn<T, TValue>(this RxDataGrid<T> grid, string header, Expression<Func<T, TValue>> property, DataGridLength? width = null, bool isReadOnly = false)
            where T : IRxObject
        {
            var column = new DataGridTextColumn { Header = header, IsReadOnly = isReadOnly };
            column.Binding = new Binding(property.GetPropertyInfo().Name) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            if (width != null)
                column.Width = width.Value;
            grid.Columns.Add(column);
            return column;
        }

        public static DataGridComboBoxColumn AddComboBoxColumn<T, TValue>(this RxDataGrid<T> grid, string header, Expression<Func<T, TValue>> property, DataGridLength? width = null)
            where T : IRxObject
        {
            var column = new DataGridComboBoxColumn { Header = header };
            column.SelectedValueBinding = new Binding(property.GetPropertyInfo().Name) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            if (width != null)
                column.Width = width.Value;
            grid.Columns.Add(column);
            return column;
        }

        public static void To<TModel, TItem>(this RxViewObjectBinder<TModel, TItem> binder, DataGridComboBoxColumn column)
            where TModel : IRxObject
            where TItem : class, IEnumerable
        {
            binder.To(x =>
            {
                if (column.ItemsSource != x)
                    column.ItemsSource = x;
            });
        }
    }
}