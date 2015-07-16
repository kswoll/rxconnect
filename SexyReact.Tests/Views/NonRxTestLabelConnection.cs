using System;
using System.Linq.Expressions;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    public static class NonRxTestLabelConnection
    {
        public static IDisposable Biconnect<TModel, TModelValue>(
            this IRxViewObject<TModel> view, 
            NonRxTestLabel label, 
            Expression<Func<TModel, TModelValue>> modelProperty,
            Func<TModelValue, string> toViewValue = null,
            Func<string, TModelValue> toModelValue = null
        )
            where TModel : IRxObject
        {
            var rxifiedLabel = new NonRxTestLabelWrapper(label);
            return view.Biconnect(rxifiedLabel, x => x.Text, modelProperty, toViewValue, toModelValue);
        }

        private class NonRxTestLabelWrapper : RxObject
        {
            public string Text { get { return Get<string>(); } set { Set(value); } }

            public NonRxTestLabelWrapper(NonRxTestLabel label)
            {
                this.ObserveProperty(x => x.Text).Subscribe(x => label.Text = x);
                label.TextChanged += (sender, args) => Text = label.Text;
            }
        }
    }
}