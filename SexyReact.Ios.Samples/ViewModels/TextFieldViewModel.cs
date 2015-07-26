using System;
using SexyReact.Fody.Helpers;

namespace SexyReact.Ios.Samples.ViewModels
{
    public class TextFieldViewModel : RxObject
    {
        [Rx]public string StringProperty { get; set; }

        public TextFieldViewModel()
        {
        }
    }
}

