using System;
using System.Threading.Tasks;
using Foundation;

namespace SexyReact.Ios
{
    public class RxIos 
    {
        static RxIos() 
        {
            Rx.UiScheduler = IosUiScheduler.Instance;
        }

        [Preserve]
        public static void RegisterDependency()
        {
        }
    }
}

