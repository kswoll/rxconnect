using System;
using System.Threading.Tasks;

namespace SexyReact.Ios
{
    public class RxIos : Attribute
    {
        static RxIos()
        {
            Rx.UiScheduler = IosUiScheduler.Instance;
        }
    }
}

