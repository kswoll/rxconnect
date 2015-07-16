using System;

namespace SexyReact.Tests.Views
{
    public class NonRxTestLabel
    {
        public event EventHandler TextChanged;

        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    OnTextChanged();
                }
            }
        }

        protected void OnTextChanged()
        {
            if (TextChanged != null)
                TextChanged(this, new EventArgs());
        }
    }
}