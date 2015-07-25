using System;
using SexyReact.Views;

namespace SexyReact.Tests.Views
{
    public static class NonRxTestLabelConnection
    {
        public static IDisposable Mate<TModel, TModelValue>(
            this RxViewObjectBinder<TModel, TModelValue> binder,
            NonRxTestLabel label, 
            Func<TModelValue, string> toViewValue = null,
            Func<string, TModelValue> toModelValue = null
        )
            where TModel : IRxObject
        {
            var rxifiedLabel = new NonRxTestLabelWrapper(label);
            return binder.Mate(rxifiedLabel, x => x.Text, toViewValue, toModelValue);
        }

        private class NonRxTestLabelWrapper : RxObject
        {
            public string Text { get { return Get<string>(); } set { Set(value); } }

            private NonRxTestLabel label;
            private IDisposable subscription;

            public NonRxTestLabelWrapper(NonRxTestLabel label)
            {
                this.label = label;
                subscription = this.ObserveProperty(x => x.Text).Subscribe(x => label.Text = x);
                label.TextChanged += UpdateText;
            }

            private void UpdateText(object sender, EventArgs args)
            {
                Text = label.Text;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (isDisposing)
                {
                    subscription.Dispose();
                    label.TextChanged -= UpdateText;
                }
            }
        }
    }
}